#!/usr/bin/env python3
"""
md_to_itch_html.py

Single-source-of-truth generator for mod pages. Each mod directory holds
one authored file (default name: page.md) containing markdown for BOTH the
GitHub README and the itch.io description. The script renders that one
source into two files:

    <scan_root>/MyMod/page.md
        -> <scan_root>/MyMod/README.md                (GitHub-facing)
        -> <script_dir>/MyMod/description.html         (itch.io-facing)

Marking content for only one target
------------------------------------
Wrap a section in an HTML comment pair naming the target. Comments are
invisible both in GitHub's rendered markdown and in the itch.io HTML, so
the markers themselves never show up anywhere:

    <!-- itch-only -->
    This paragraph (images, tables, whatever) only ends up on itch.io.
    <!-- /itch-only -->

    <!-- readme-only -->
    This paragraph only ends up in README.md.
    <!-- /readme-only -->

Works inline too:

    Available <!-- itch-only -->here on itch.io<!-- /itch-only --><!-- readme-only -->on itch.io and Steam<!-- /readme-only -->.

Anything outside a marker pair appears in both outputs. Markers don't
nest. A marker left without its matching close (or vice versa) is reported
as a warning and left as literal text rather than silently eaten.

Local image/link rewriting still applies to the itch.io output only (see
below) -- the README.md output keeps ordinary relative paths, since GitHub
already resolves those against the repo natively.

Both generated files get an HTML-comment banner at the top noting they're
generated, from which source file, and when (invisible when rendered, both
on GitHub and on itch.io).

Any locally-relative reference in the itch-facing markdown (an image, a
link to another file in the repo, etc.) is rewritten to an absolute
raw.githubusercontent.com URL, since those local paths would otherwise be
dead once the HTML is pasted into itch.io. This requires knowing where the
repo root is, which is auto-detected via git if this script is run from
inside the checkout (override with --repo-root / --raw-base if needed).

Usage:
    python md_to_itch_html.py [options]

Options:
    --root PATH          Override the "two levels above" scan root.
    --source-name NAME    Filename of the shared source (default: page.md).
    --readme-name NAME    Filename to write the README-facing output as
                          (default: README.md).
    --out-dir PATH        Override where per-mod description.html folders
                          are created (defaults to this script's own
                          directory).
    --repo-root PATH      Path to the git repo root (auto-detected by
                          default).
    --raw-base URL        Base raw.githubusercontent.com URL local
                          references are resolved against.
    --dry-run            Show what would happen without writing any files.
    -v, --verbose         Print extra detail while converting.

Requires:
    pip install markdown pymdown-extensions beautifulsoup4
"""

from __future__ import annotations

import argparse
import re
import sys
from datetime import datetime, timezone
from pathlib import Path

try:
    import markdown
except ImportError:
    sys.exit(
        "Missing dependency 'markdown'. Install with:\n"
        "    pip install markdown pymdown-extensions beautifulsoup4"
    )

try:
    from bs4 import BeautifulSoup
except ImportError:
    sys.exit(
        "Missing dependency 'beautifulsoup4'. Install with:\n"
        "    pip install markdown pymdown-extensions beautifulsoup4"
    )


# ---------------------------------------------------------------------------
# Single-source directives: <!-- itch-only --> / <!-- readme-only -->
# ---------------------------------------------------------------------------

_ONLY_BLOCK_RE = re.compile(
    r"<!--\s*(itch|readme)-only\s*-->(.*?)<!--\s*/\1-only\s*-->",
    re.DOTALL | re.IGNORECASE,
)
_STRAY_MARKER_RE = re.compile(r"<!--\s*/?(?:itch|readme)-only\s*-->", re.IGNORECASE)


def extract_for_target(source_text: str, target: str) -> tuple[str, list[str]]:
    """Strip <!-- itch-only -->/<!-- readme-only --> blocks not meant for
    `target` ("itch" or "readme"), unwrapping the ones that are. Returns
    (text, warnings); a warning is emitted if any marker is left dangling
    (unmatched open or close) rather than silently dropping content."""

    def _repl(match: "re.Match[str]") -> str:
        kind = match.group(1).lower()
        body = match.group(2)
        return body if kind == target else ""

    result = _ONLY_BLOCK_RE.sub(_repl, source_text)

    stray = _STRAY_MARKER_RE.findall(result)
    warnings = []
    if stray:
        warnings.append(
            f"{len(stray)} unmatched itch-only/readme-only marker(s) found "
            f"(missing its opening or closing tag?) -- left as literal text"
        )
    return result, warnings


def _tidy_blank_lines(text: str) -> str:
    """Collapse runs of blank lines left behind by removed sections, and
    normalize surrounding whitespace."""
    text = re.sub(r"\n{3,}", "\n\n", text)
    return text.strip() + "\n"


def render_markdown_for_target(source_text: str, target: str) -> tuple[str, list[str]]:
    text, warnings = extract_for_target(source_text, target)
    return _tidy_blank_lines(text), warnings


# ---------------------------------------------------------------------------
# Markdown -> HTML conversion
# ---------------------------------------------------------------------------

# Extensions chosen to cover everything markdown.css has rules for:
#   tables               -> table/thead/tbody/th/td  (zebra striping, no borders)
#   def_list              -> dl/dt/dd
#   pymdownx.tilde         -> ~~strike~~ -> <del>
#   pymdownx.tasklist      -> - [ ] / - [x] -> <input type="checkbox">
#   pymdownx.superfences    -> fenced code blocks -> pre > code
#   footnotes, abbr, attr_list -> general GFM-ish niceties
#   sane_lists             -> more predictable list parsing
#   toc                    -> adds ids to headings (nice for anchor links);
#                              permalinks disabled so it doesn't inject markup
#                              markdown.css doesn't style.
#   Raw HTML (e.g. hand-written <details><summary>) passes through
#   python-markdown untouched, which covers the details/summary styling
#   without needing a dedicated extension.

MD_EXTENSIONS = [
    "md_in_html",
    "tables",
    "def_list",
    "footnotes",
    "abbr",
    "attr_list",
    "sane_lists",
    "toc",
    "pymdownx.tilde",
    "pymdownx.tasklist",
    "pymdownx.superfences",
    "pymdownx.highlight",
]

MD_EXTENSION_CONFIGS = {
    "pymdownx.tasklist": {
        # Plain <input type="checkbox">, matching the
        # `input[type="checkbox"]` rule in markdown.css, rather than
        # pymdown's icon-font checkboxes.
        "custom_checkbox": False,
        "clickable_checkbox": False,
    },
    "toc": {
        "permalink": False,
    },
    "pymdownx.highlight": {
        # markdown.css only styles plain pre > code (background, padding,
        # radius) and has no Pygments theme, so keep fenced code blocks as
        # simple <pre><code> instead of Pygments' <div class="highlight">
        # + syntax-highlighting spans, which would render as unstyled
        # (colorless) markup on itch.io without an accompanying theme.
        "use_pygments": False,
    },
}


import subprocess
import posixpath
from urllib.parse import urlsplit, urlunsplit, quote

# python-markdown treats raw HTML blocks as opaque by default: markdown
# syntax *inside* a hand-written <details> (or other block tag) is left
# untouched unless that tag carries markdown="1". Authors writing plain
# <details><summary>...</summary>- list items</details> almost always
# expect the inside to still be parsed as markdown, so we inject the
# attribute automatically for the block tags markdown.css styles.
_RAW_HTML_BLOCK_TAGS = ("details", "div", "section", "article", "aside")
_RAW_HTML_OPEN_TAG_RE = re.compile(
    r"<(" + "|".join(_RAW_HTML_BLOCK_TAGS) + r")((?:\s+[^>]*)?)>",
    re.IGNORECASE,
)


def _enable_markdown_inside_raw_html_blocks(md_text: str) -> str:
    def _add_attr(match: "re.Match[str]") -> str:
        tag, attrs = match.group(1), match.group(2)
        if re.search(r"\bmarkdown\s*=", attrs, re.IGNORECASE):
            return match.group(0)  # author already set it explicitly
        return f"<{tag}{attrs} markdown=\"1\">"

    return _RAW_HTML_OPEN_TAG_RE.sub(_add_attr, md_text)


def convert_markdown_to_html(md_text: str) -> str:
    """Convert raw markdown text into the inner HTML for .custom-markdown."""
    md_text = _enable_markdown_inside_raw_html_blocks(md_text)
    html_body = markdown.markdown(
        md_text,
        extensions=MD_EXTENSIONS,
        extension_configs=MD_EXTENSION_CONFIGS,
        output_format="html5",
    )
    html_body = _wrap_standalone_images_in_figures(html_body)
    return html_body


def _wrap_standalone_images_in_figures(html_body: str) -> str:
    """
    markdown.css styles <figure>/<figcaption>, but python-markdown emits a
    bare <img> (wrapped in <p>) for image syntax. Any image that is the
    sole content of its paragraph gets promoted to a <figure>, using the
    image's title (or failing that, its alt text) as the caption.
    """
    soup = BeautifulSoup(html_body, "html.parser")

    for p in soup.find_all("p"):
        non_whitespace_children = [
            c for c in p.contents if not (isinstance(c, str) and not c.strip())
        ]
        if len(non_whitespace_children) == 1 and getattr(
            non_whitespace_children[0], "name", None
        ) == "img":
            img = non_whitespace_children[0]
            caption_text = (img.get("title") or img.get("alt") or "").strip()

            figure = soup.new_tag("figure")
            img.extract()
            figure.append(img)

            if caption_text:
                figcaption = soup.new_tag("figcaption")
                figcaption.string = caption_text
                figure.append(figcaption)

            p.replace_with(figure)

    return str(soup)


# ---------------------------------------------------------------------------
# Local reference -> GitHub raw URL rewriting
# ---------------------------------------------------------------------------
#
# Once this HTML lives on itch.io, any path relative to the README (a
# screenshot, a link to another doc in the repo, etc.) is dead. But because
# this script itself lives in the GitHub repo, we know exactly where each
# README sits relative to the repo root, so any local reference can be
# rewritten into a stable raw.githubusercontent.com URL, e.g.:
#
#   https://raw.githubusercontent.com/<org>/<repo>/refs/heads/<branch>/<path-to-readme-dir>/<relative-ref>

DEFAULT_GITHUB_RAW_BASE = (
    "https://raw.githubusercontent.com/onesuchkeeper/HuniePop2-BepInEx-Mods/refs/heads/main/"
)

_SKIP_SCHEMES = {"http", "https", "data", "mailto", "tel", "javascript"}


def find_repo_root(start: Path) -> Path | None:
    """Locate the git repository root containing `start`, if any."""
    try:
        result = subprocess.run(
            ["git", "-C", str(start), "rev-parse", "--show-toplevel"],
            capture_output=True,
            text=True,
            check=True,
        )
    except (OSError, subprocess.CalledProcessError):
        return None
    top = result.stdout.strip()
    return Path(top).resolve() if top else None


def _is_local_reference(raw_value: str) -> bool:
    """True for src/href values that point at a local file rather than an
    external resource, an in-page anchor, or a non-http(s) protocol."""
    if not raw_value:
        return False
    if raw_value.startswith("#"):
        return False
    parsed = urlsplit(raw_value)
    if parsed.scheme and parsed.scheme.lower() in _SKIP_SCHEMES:
        return False
    if parsed.netloc:
        # protocol-relative ("//example.com/x") or has a scheme we didn't
        # already exclude above (still an external host either way)
        return False
    return bool(parsed.path)


def _quote_url_path(url: str) -> str:
    """Percent-encode spaces/unicode in the path of a URL without
    double-encoding anything already percent-encoded."""
    parts = urlsplit(url)
    quoted_path = quote(parts.path, safe="/%")
    return urlunsplit((parts.scheme, parts.netloc, quoted_path, parts.query, parts.fragment))


def resolve_local_reference(
    raw_value: str, readme_repo_rel_dir: str, raw_base: str
) -> tuple[str, bool]:
    """
    Resolve a local src/href value found in a README into an absolute
    raw.githubusercontent.com URL.

    `readme_repo_rel_dir` is the POSIX path of the README's own directory,
    relative to the repo root (e.g. "ItchPages/Hp2BaseMod"), or "" if the
    README sits at the repo root.

    Returns (resolved_url, escapes_repo_root). If escapes_repo_root is
    True, the reference climbed above the repo root via "../" and the
    caller should warn rather than trust the URL.
    """
    parts = urlsplit(raw_value)
    path = parts.path

    if path.startswith("/"):
        # Path is already meant to be relative to the repo root.
        target = posixpath.normpath(path.lstrip("/"))
    else:
        target = posixpath.normpath(posixpath.join(readme_repo_rel_dir, path))

    escapes_repo_root = target == ".." or target.startswith("../")
    if target == ".":
        target = ""

    base = raw_base if raw_base.endswith("/") else raw_base + "/"
    resolved = base + target
    resolved = urlunsplit(
        (urlsplit(resolved).scheme, urlsplit(resolved).netloc, urlsplit(resolved).path,
         parts.query, parts.fragment)
    )
    return _quote_url_path(resolved), escapes_repo_root


def rewrite_local_references(
    html_body: str, readme_repo_rel_dir: str, raw_base: str
) -> tuple[str, list[str]]:
    """Rewrite local <img src> and <a href> values to raw.githubusercontent.com
    URLs. Returns (new_html, warnings) where warnings describe any
    reference that couldn't be safely resolved (e.g. it climbs above the
    repo root)."""
    soup = BeautifulSoup(html_body, "html.parser")
    warnings: list[str] = []

    for tag_name, attr in (("img", "src"), ("a", "href")):
        for tag in soup.find_all(tag_name):
            value = tag.get(attr)
            if not value or not _is_local_reference(value):
                continue
            resolved, escapes = resolve_local_reference(value, readme_repo_rel_dir, raw_base)
            if escapes:
                warnings.append(
                    f'{tag_name}[{attr}="{value}"] resolves above the repo root; left unchanged'
                )
                continue
            tag[attr] = resolved

    return str(soup), warnings


def find_local_references(html_body: str) -> list[str]:
    """Fallback used when the repo root can't be determined: just report
    local img/a references instead of rewriting them."""
    soup = BeautifulSoup(html_body, "html.parser")
    found = []
    for tag_name, attr in (("img", "src"), ("a", "href")):
        for tag in soup.find_all(tag_name):
            value = tag.get(attr)
            if value and _is_local_reference(value):
                found.append(f"{tag_name}[{attr}] -> {value}")
    return found


def build_description_html(
    md_text: str, readme_repo_rel_dir: str | None, raw_base: str
) -> tuple[str, list[str]]:
    """Return (full description.html contents, list of warning strings).

    If `readme_repo_rel_dir` is None (repo root couldn't be determined),
    local references are left as-is and merely reported as warnings.
    """
    inner_html = convert_markdown_to_html(md_text)

    if readme_repo_rel_dir is None:
        local_refs = find_local_references(inner_html)
        warnings = [
            f"repo root not found; left local reference unresolved: {ref}"
            for ref in local_refs
        ]
    else:
        inner_html, warnings = rewrite_local_references(inner_html, readme_repo_rel_dir, raw_base)

    full_html = f'<div class="custom-markdown">\n{inner_html}\n</div>\n'
    return full_html, warnings


# ---------------------------------------------------------------------------
# Directory scanning / file I/O
# ---------------------------------------------------------------------------

SCRIPT_NAME = Path(__file__).name


def _generated_banner_lines(source_display: str, generated_at: str) -> list[str]:
    return [
        "GENERATED FILE -- DO NOT EDIT DIRECTLY.",
        f"Generated by {SCRIPT_NAME} from {source_display}",
        f"Generated at: {generated_at}",
        "Edit the source file instead and re-run the generator.",
    ]


def _html_comment_block(lines: list[str]) -> str:
    body = "\n".join(f"  {line}" for line in lines)
    return f"<!--\n{body}\n-->"


def _source_display_path(source_path: Path, repo_root: Path | None, scan_root: Path) -> str:
    """Best-effort human-readable path to the source file for the banner:
    repo-root-relative if we know it, else scan-root-relative, else just
    the filename."""
    resolved = source_path.resolve()
    for base in (repo_root, scan_root):
        if base is not None:
            try:
                return resolved.relative_to(base).as_posix()
            except ValueError:
                continue
    return source_path.name


def find_source_files(scan_root: Path, source_name: str) -> list[Path]:
    """Find `source_name` (case-insensitive) directly inside each immediate
    sub-directory of scan_root."""
    sources = []
    if not scan_root.is_dir():
        return sources

    candidate_names = {source_name, source_name.lower(), source_name.upper(), source_name.title()}

    for entry in sorted(scan_root.iterdir()):
        if not entry.is_dir():
            continue
        for candidate_name in candidate_names:
            candidate = entry / candidate_name
            if candidate.is_file():
                sources.append(candidate)
                break
    return sources


def process_mod_page(
    source_path: Path,
    out_dir: Path,
    repo_root: Path | None,
    scan_root: Path,
    raw_base: str,
    readme_name: str,
    generated_at: str,
    dry_run: bool,
    verbose: bool,
) -> None:
    mod_name = source_path.parent.name
    mod_dir = source_path.parent
    readme_dest = mod_dir / readme_name
    itch_dest_dir = out_dir / mod_name
    itch_dest_file = itch_dest_dir / "description.html"

    source_text = source_path.read_text(encoding="utf-8")

    readme_text, readme_warnings = render_markdown_for_target(source_text, "readme")
    itch_markdown, itch_warnings = render_markdown_for_target(source_text, "itch")

    repo_rel_dir = None
    if repo_root is not None:
        repo_rel_dir = mod_dir.resolve().relative_to(repo_root).as_posix()
        if repo_rel_dir == ".":
            repo_rel_dir = ""

    itch_html, rewrite_warnings = build_description_html(itch_markdown, repo_rel_dir, raw_base)

    source_display = _source_display_path(source_path, repo_root, scan_root)
    banner = _html_comment_block(_generated_banner_lines(source_display, generated_at))
    readme_text = f"{banner}\n\n{readme_text}"
    itch_html = f"{banner}\n{itch_html}"

    warnings = (
        [f"[readme] {w}" for w in readme_warnings]
        + [f"[itch]   {w}" for w in itch_warnings]
        + [f"[itch]   {w}" for w in rewrite_warnings]
    )

    if verbose or warnings:
        print(f"[{mod_name}] {source_path}")
        print(f"    -> {readme_dest}")
        print(f"    -> {itch_dest_file}")
    for warning in warnings:
        print(f"  ! {warning}")

    if dry_run:
        return

    readme_dest.write_text(readme_text, encoding="utf-8")
    itch_dest_dir.mkdir(parents=True, exist_ok=True)
    itch_dest_file.write_text(itch_html, encoding="utf-8")


# ---------------------------------------------------------------------------
# Entry point
# ---------------------------------------------------------------------------

def parse_args(argv: list[str]) -> argparse.Namespace:
    script_dir = Path(__file__).resolve().parent
    default_root = script_dir.parent

    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "--root",
        type=Path,
        default=default_root,
        help=f"Directory to scan for */<source-name> files (default: {default_root})",
    )
    parser.add_argument(
        "--source-name",
        default="page.md",
        help="Filename of the shared source markdown in each mod directory "
             "(default: page.md). This must NOT be README.md itself -- "
             "writing the stripped-down README back over the only copy of "
             "the source would permanently delete the itch-only content.",
    )
    parser.add_argument(
        "--readme-name",
        default="README.md",
        help="Filename to write the README-facing output as, next to the "
             "source file (default: README.md).",
    )
    parser.add_argument(
        "--out-dir",
        type=Path,
        default=script_dir,
        help=f"Where to create <ModName>/description.html output folders "
             f"(default: this script's directory, {script_dir})",
    )
    parser.add_argument(
        "--dry-run",
        action="store_true",
        help="Report what would be converted without writing files.",
    )
    parser.add_argument(
        "--repo-root",
        type=Path,
        default=None,
        help="Path to the git repo root, used to compute each source file's "
             "location for rewriting local references. Auto-detected via "
             "`git rev-parse --show-toplevel` if not given.",
    )
    parser.add_argument(
        "--raw-base",
        default=DEFAULT_GITHUB_RAW_BASE,
        help=f"Base raw.githubusercontent.com URL local references are "
             f"resolved against (default: {DEFAULT_GITHUB_RAW_BASE})",
    )
    parser.add_argument(
        "-v", "--verbose",
        action="store_true",
        help="Print each source file found/converted.",
    )
    return parser.parse_args(argv)


def main(argv: list[str]) -> int:
    args = parse_args(argv)
    scan_root = args.root.resolve()
    out_dir = args.out_dir.resolve()

    if args.source_name.lower() == args.readme_name.lower():
        sys.exit(
            "--source-name and --readme-name can't be the same file: the "
            "source needs to survive independently of the generated README, "
            "or itch-only content would be lost the moment README.md is "
            "regenerated."
        )

    repo_root = args.repo_root.resolve() if args.repo_root else find_repo_root(scan_root)
    if repo_root is None:
        print(
            "! Could not determine the git repo root (not a git checkout, or "
            "git isn't available). Local image/link references will be left "
            "as-is and only reported, not rewritten. Pass --repo-root to fix "
            "this."
        )
    elif args.verbose:
        print(f"Repo root: {repo_root}")
        print(f"Raw base:  {args.raw_base}")

    sources = find_source_files(scan_root, args.source_name)

    if not sources:
        print(f"No {args.source_name} files found in sub-directories of {scan_root}")
        return 0

    generated_at = datetime.now(timezone.utc).strftime("%Y-%m-%d %H:%M:%S UTC")

    print(f"Scanning {scan_root} -> found {len(sources)} {args.source_name} file(s)")
    for source_path in sources:
        process_mod_page(
            source_path, out_dir, repo_root, scan_root, args.raw_base, args.readme_name,
            generated_at, args.dry_run, args.verbose,
        )

    if args.dry_run:
        print("(dry run: no files were written)")
    else:
        print(f"Done. description.html written under {out_dir}; "
              f"README.md written next to each {args.source_name}")

    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv[1:]))