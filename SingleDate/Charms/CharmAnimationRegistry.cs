using System.Collections.Generic;
using DG.Tweening;
using Hp2BaseMod;
using UnityEngine;
using System.Linq;
using HarmonyLib;
using System;

namespace SingleDate;

public class CharmAnimationRegistry
{
    private readonly List<CharmAnimationDefinition> _animations = new();

    public CharmAnimationRegistry()
    {
        RegisterAll();
    }

    public Sequence CreateSequence(CharmAnimationContext context, float budget)
    {
        var seq = DOTween.Sequence();
        float remaining = budget;

        int safety = 0;

        while (remaining > 0.01f && safety++ < 20)
        {
            var pool = GetValidAnimations(context.GirlId, remaining);

            if (pool.Count == 0)
            {
                seq.AppendInterval(0.5f);
                break;
            }

            var anim = WeightedPick(pool);

            remaining -= anim.Cost;

            anim.Build(seq, context);
        }

        return seq;
    }

    private List<CharmAnimationDefinition> GetValidAnimations(RelativeId id, float budget)
    {
        var result = new List<CharmAnimationDefinition>();

        foreach (var a in _animations)
        {
            if (a.Cost > budget)
                continue;

            if (a.AllowedCharacters != null && !a.AllowedCharacters.Contains(id))
                continue;

            result.Add(a);
        }

        return result;
    }

    private CharmAnimationDefinition WeightedPick(List<CharmAnimationDefinition> list)
    {
        float total = 0f;

        foreach (var a in list) total += a.Weight;

        float roll = UnityEngine.Random.value * total;

        foreach (var a in list)
        {
            roll -= a.Weight;
            if (roll <= 0) return a;
        }

        return list[list.Count - 1];
    }

    public void Register(
        float weight, 
        float cost,
        Func<Sequence, (RectTransform transform, bool dir, RelativeId girlId), bool> build,
        HashSet<RelativeId> allowedCharacters)
    {
        _animations.Add(new CharmAnimationDefinition
        {
            Weight = weight,
            Cost = cost,
            AllowedCharacters = allowedCharacters,
            Build = (seq, c) =>
            {
                if (build(seq, (c.Transform, c.Dir > 0, c.GirlId)))
                {
                    c.Dir *= -1;
                }
            }
        });
    }

    private void RegisterAll()
    {
        // =========================================================
        // CORE GENERIC MOTION PRIMITIVES
        // =========================================================

        _animations.Add(new CharmAnimationDefinition
        {
            Weight = 1.4f,
            Cost = 1.0f,
            AllowedCharacters = null,
            Build = (seq, c) =>
            {
                // chaotic lateral displacement (random impulse)
                seq.Append(c.Transform.DOLocalJump(
                    c.Transform.localPosition + new Vector3(UnityEngine.Random.Range(-60f, 60f), 0),
                    UnityEngine.Random.Range(10f, 20f),
                    UnityEngine.Random.Range(1, 2),
                    1.2f).SetEase(Ease.Linear));
            }
        });

        _animations.Add(new CharmAnimationDefinition
        {
            Weight = 0.8f,
            Cost = 0.15f,
            AllowedCharacters = null,
            Build = (seq, c) =>
            {
                // instant directional inversion
                c.Dir *= -1;
            }
        });

        _animations.Add(new CharmAnimationDefinition
        {
            Weight = 1.0f,
            Cost = 0.6f,
            AllowedCharacters = null,
            Build = (seq, c) =>
            {
                // elastic compression response
                seq.Join(c.Transform.DOScaleY(0.95f, 1f).SetEase(Ease.InOutElastic));
                seq.Join(c.Transform.DOScaleX(1.05f * c.Dir, 1f).SetEase(Ease.InOutElastic));
            }
        });

        _animations.Add(new CharmAnimationDefinition
        {
            Weight = 1.0f,
            Cost = 0.6f,
            AllowedCharacters = null,
            Build = (seq, c) =>
            {
                // inverse elastic expansion
                seq.Join(c.Transform.DOScaleY(1.05f, 1f).SetEase(Ease.InOutElastic));
                seq.Join(c.Transform.DOScaleX(0.95f * c.Dir, 1.2f).SetEase(Ease.InOutElastic));
            }
        });

        _animations.Add(new CharmAnimationDefinition
        {
            Weight = 1.1f,
            Cost = 0.8f,
            AllowedCharacters = null,
            Build = (seq, c) =>
            {
                // inversion flip (identity disruption)
                seq.Append(c.Transform.DOScaleX(-1f * c.Dir, 1).SetEase(Ease.InSine));
                seq.Append(c.Transform.DOScaleX(1f * c.Dir, 1).SetEase(Ease.OutSine));
            }
        });

        _animations.Add(new CharmAnimationDefinition
        {
            Weight = 0.5f,
            Cost = 3.5f,
            AllowedCharacters = null,
            Build = (seq, c) =>
            {
                // full impulse buildup
                seq.Append(c.Transform.DOScaleX(1.4f * c.Dir, 1.2f));
                seq.Join(c.Transform.DOScaleY(0.75f, 1.2f));
                seq.AppendInterval(0.45f);

                // jump impulse
                seq.Append(c.Transform.DOLocalJump(c.Transform.localPosition, 200f, 1, 3.5f));

                // spin decay
                seq.Append(c.Transform.DORotate(new Vector3(0, 0, c.Dir * 360f), 2.3f, RotateMode.FastBeyond360));

                seq.Append(c.Transform.DOScale(1f, 1f));
            }
        });

        _animations.Add(new CharmAnimationDefinition
        {
            Weight = 1.0f,
            Cost = 0.7f,
            AllowedCharacters = null,
            Build = (seq, c) =>
            {
                // micro recoil pulse
                seq.Append(c.Transform.DOScale(1.08f, 0.12f));
                seq.Append(c.Transform.DOScale(1f, 0.18f));
            }
        });

        _animations.Add(new CharmAnimationDefinition
        {
            Weight = 0.9f,
            Cost = 0.9f,
            AllowedCharacters = null,
            Build = (seq, c) =>
            {
                // hesitation step
                seq.Append(c.Transform.DOLocalMoveX(
                    c.Transform.localPosition.x + (12f * c.Dir), 0.25f));

                seq.AppendInterval(0.4f);

                seq.Append(c.Transform.DOLocalMoveX(c.Transform.localPosition.x, 0.35f));
            }
        });

        _animations.Add(new CharmAnimationDefinition
        {
            Weight = 0.85f,
            Cost = 1.0f,
            AllowedCharacters = null,
            Build = (seq, c) =>
            {
                // vertical bounce settle
                seq.Append(c.Transform.DOLocalMoveY(
                    c.Transform.localPosition.y + 8f, 0.2f));

                seq.Append(c.Transform.DOLocalMoveY(
                    c.Transform.localPosition.y, 0.4f));
            }
        });

        _animations.Add(new CharmAnimationDefinition
        {
            Weight = 1.05f,
            Cost = 1.1f,
            AllowedCharacters = null,
            Build = (seq, c) =>
            {
                // emotional wobble drift
                seq.Append(c.Transform.DOShakePosition(0.8f, 10f, 12));

                seq.Append(c.Transform.DOLocalMove(c.Transform.localPosition, 0.1f));
            }
        });

        // =========================================================
        // CHARACTER-SPECIFIC ANIMATIONS
        // =========================================================

        // -------------------------
        // Sarah
        // -------------------------
        _animations.Add(new CharmAnimationDefinition
        {
            Weight = 1.35f,
            Cost = 1.2f,
            AllowedCharacters = new HashSet<RelativeId> { Hp2BaseMod.Girls.Sarah },
            Build = (seq, c) =>
            {
                seq.Append(c.Transform.DOScale(1.25f, 0.15f));
                seq.Append(c.Transform.DOScale(0.9f, 0.15f));
                seq.Append(c.Transform.DOScale(1.15f, 0.2f));

                seq.Append(c.Transform.DOLocalMoveX(c.Transform.localPosition.x + (30f * c.Dir), 0.25f));
                seq.Append(c.Transform.DOLocalMoveX(c.Transform.localPosition.x, 0.35f));

                seq.Append(c.Transform.DOShakeAnchorPos(0.8f, 12f, 14));
            }
        });

        _animations.Add(new CharmAnimationDefinition
        {
            Weight = 1.35f,
            Cost = 0.9f,
            AllowedCharacters = new HashSet<RelativeId> { Hp2BaseMod.Girls.Sarah },
            Build = (seq, c) =>
            {
                seq.Append(c.Transform.DOScale(1.22f, 0.2f));
                seq.Append(c.Transform.DOScale(0.92f, 0.2f));

                seq.Append(c.Transform.DOShakeAnchorPos(0.8f, 14f, 18));

                seq.Append(c.Transform.DOLocalMoveX(
                    c.Transform.localPosition.x + (22f * c.Dir), 0.25f));

                seq.Append(c.Transform.DOLocalMoveX(c.Transform.localPosition.x, 0.35f));
            }
        });

        // -------------------------
        // Lillian
        // -------------------------
        _animations.Add(new CharmAnimationDefinition
        {
            Weight = 0.85f,
            Cost = 1.2f,
            AllowedCharacters = new HashSet<RelativeId> { Hp2BaseMod.Girls.Lillian },
            Build = (seq, c) =>
            {
                seq.Append(c.Transform.DOLocalMoveY(c.Transform.localPosition.y - 16f, 1.4f));
                seq.AppendInterval(2.5f);
                seq.Append(c.Transform.DOLocalMoveY(c.Transform.localPosition.y - 6f, 1.6f));
                seq.Join(c.Transform.DOScale(0.98f, 1.6f));
            }
        });

        _animations.Add(new CharmAnimationDefinition
        {
            Weight = 0.8f,
            Cost = 1.4f,
            AllowedCharacters = new HashSet<RelativeId> { Hp2BaseMod.Girls.Lillian },
            Build = (seq, c) =>
            {
                seq.Append(c.Transform.DOLocalMoveY(c.Transform.localPosition.y - 10f, 1.2f));
                seq.AppendInterval(2.0f);
                seq.Append(c.Transform.DOLocalMoveY(c.Transform.localPosition.y - 3f, 1.3f));
                seq.Join(c.Transform.DORotate(new Vector3(0, 0, -3f * c.Dir), 1.3f));
            }
        });

        // -------------------------
        // Abia
        // -------------------------
        _animations.Add(new CharmAnimationDefinition
        {
            Weight = 0.8f,
            Cost = 1.5f,
            AllowedCharacters = new HashSet<RelativeId> { Hp2BaseMod.Girls.Abia },
            Build = (seq, c) =>
            {
                seq.Append(c.Transform.DOShakeAnchorPos(4f, 30f));

                seq.Append(c.Transform.DOScale(1.1f, 0.2f));
                seq.Append(c.Transform.DOScale(1f, 0.4f));

                seq.Append(c.Transform.DOShakePosition(0.8f, 6f, 20));
            }
        });

        _animations.Add(new CharmAnimationDefinition
        {
            Weight = 1.0f,
            Cost = 1.2f,
            AllowedCharacters = new HashSet<RelativeId> { Hp2BaseMod.Girls.Abia },
            Build = (seq, c) =>
            {
                seq.Append(c.Transform.DOScale(1.08f, 0.12f));
                seq.Join(c.Transform.DOScaleX(0.92f * c.Dir, 0.12f));

                seq.Append(c.Transform.DOLocalMoveX(c.Transform.localPosition.x - (12f * c.Dir), 0.2f));
                seq.Append(c.Transform.DOLocalMoveX(c.Transform.localPosition.x, 0.35f));
            }
        });

        // -------------------------
        // Ashley
        // -------------------------
        _animations.Add(new CharmAnimationDefinition
        {
            Weight = 1.25f,
            Cost = 1.1f,
            AllowedCharacters = new HashSet<RelativeId> { Hp2BaseMod.Girls.Ashley },
            Build = (seq, c) =>
            {
                seq.Append(c.Transform.DOLocalMoveX(c.Transform.localPosition.x + (20f * c.Dir), 0.2f));
                seq.Join(c.Transform.DOScale(1.1f, 0.2f));

                seq.Append(c.Transform.DOLocalMoveX(c.Transform.localPosition.x, 0.25f));

                seq.Append(c.Transform.DOLocalMoveX(c.Transform.localPosition.x + (10f * c.Dir), 0.2f));
                seq.Append(c.Transform.DOLocalMoveX(c.Transform.localPosition.x, 0.25f));

                seq.Append(c.Transform.DOShakeAnchorPos(0.5f, 9f, 12));
            }
        });

        _animations.Add(new CharmAnimationDefinition
        {
            Weight = 1.3f,
            Cost = 1.0f,
            AllowedCharacters = new HashSet<RelativeId> { Hp2BaseMod.Girls.Ashley },
            Build = (seq, c) =>
            {
                // forward tease (inviting attention)
                seq.Append(c.Transform.DOLocalMoveX(
                    c.Transform.localPosition.x + (26f * c.Dir), 0.25f).SetEase(Ease.OutBack));

                seq.Join(c.Transform.DOScale(1.14f, 0.25f));

                // sudden withdrawal (playful rejection)
                seq.Append(c.Transform.DOLocalMoveX(
                    c.Transform.localPosition.x, 0.3f).SetEase(Ease.InSine));

                seq.Join(c.Transform.DOScale(1f, 0.3f));

                // after-bounce “did you follow?” shake
                seq.Append(c.Transform.DOShakeAnchorPos(0.7f, 10f, 14));

                // small secondary lean-in (reinforce teasing loop)
                seq.Append(c.Transform.DOLocalMoveX(
                    c.Transform.localPosition.x + (10f * c.Dir), 0.2f));

                seq.Append(c.Transform.DOLocalMoveX(
                    c.Transform.localPosition.x, 0.25f));
            }
        });

        // -------------------------
        // Brooke
        // -------------------------
        _animations.Add(new CharmAnimationDefinition
        {
            Weight = 1.1f,
            Cost = 1.3f,
            AllowedCharacters = new HashSet<RelativeId> { Hp2BaseMod.Girls.Brooke },
            Build = (seq, c) =>
            {
                seq.Append(c.Transform.DOLocalMoveX(c.Transform.localPosition.x + (18f * c.Dir), 0.5f));

                seq.AppendInterval(1.2f);

                seq.Append(c.Transform.DOLocalMoveX(c.Transform.localPosition.x, 0.4f));

                seq.Append(c.Transform.DOShakeAnchorPos(0.35f, 4f, 8));
            }
        });

        _animations.Add(new CharmAnimationDefinition
        {
            Weight = 1.25f,
            Cost = 1.2f,
            AllowedCharacters = new HashSet<RelativeId> { Hp2BaseMod.Girls.Brooke },
            Build = (seq, c) =>
            {
                // confident step-in (inspection posture)
                seq.Append(c.Transform.DOLocalMoveX(
                    c.Transform.localPosition.x + (22f * c.Dir), 0.35f));

                seq.Join(c.Transform.DOScale(1.12f, 0.35f));

                // deliberate pause (judgment scan)
                seq.AppendInterval(1.1f);

                // slight lean-back (dismissive reassessment)
                seq.Append(c.Transform.DOLocalMoveX(
                    c.Transform.localPosition.x - (8f * c.Dir), 0.25f));

                // return to neutral stance
                seq.Append(c.Transform.DOLocalMoveX(
                    c.Transform.localPosition.x, 0.4f));

                seq.Join(c.Transform.DOScale(1f, 0.4f));

                // subtle corrective shake (final verdict adjustment)
                seq.Append(c.Transform.DOShakeAnchorPos(0.5f, 6f, 10));
            }
        });

        // -------------------------
        // Candace
        // -------------------------
        _animations.Add(new CharmAnimationDefinition
        {
            Weight = 1.3f,
            Cost = 1.0f,
            AllowedCharacters = new HashSet<RelativeId> { Hp2BaseMod.Girls.Candace },
            Build = (seq, c) =>
            {
                seq.Append(c.Transform.DOScale(1.15f, 0.1f));
                seq.Append(c.Transform.DOScale(0.9f, 0.1f));
                seq.Append(c.Transform.DOScale(1.2f, 0.1f));

                seq.Append(c.Transform.DOShakePosition(0.9f, 16f, 18));

                seq.AppendInterval(0.6f);

                seq.Append(c.Transform.DOScale(1f, 0.6f));
            }
        });

        _animations.Add(new CharmAnimationDefinition
        {
            Weight = 1.25f,
            Cost = 1.0f,
            AllowedCharacters = new HashSet<RelativeId> { Hp2BaseMod.Girls.Candace },
            Build = (seq, c) =>
            {
                // delayed repetition confusion (echo of thought)
                seq.Append(c.Transform.DOScale(1.1f, 0.15f));
                seq.Append(c.Transform.DOScale(0.95f, 0.15f));

                // sideways uncertainty drift
                seq.Append(c.Transform.DOLocalMoveX(
                    c.Transform.localPosition.x + (8f * c.Dir), 0.25f));

                seq.Append(c.Transform.DOLocalMoveX(c.Transform.localPosition.x, 0.3f));

                // “wait what?” reset jitter
                seq.Append(c.Transform.DOShakeAnchorPos(0.6f, 10f, 12));
            }
        });

        // -------------------------
        // Jessie
        // -------------------------
        _animations.Add(new CharmAnimationDefinition
        {
            Weight = 1.0f,
            Cost = 1.3f,
            AllowedCharacters = new HashSet<RelativeId> { Hp2BaseMod.Girls.Jessie },
            Build = (seq, c) =>
            {
                seq.Append(c.Transform.DOLocalMoveY(c.Transform.localPosition.y - 12f, 1.2f));
                seq.AppendInterval(1.2f);
                seq.Append(c.Transform.DOLocalMoveY(c.Transform.localPosition.y - 4f, 1.5f));
                seq.Join(c.Transform.DOScale(0.98f, 1.5f));
            }
        });

        _animations.Add(new CharmAnimationDefinition
        {
            Weight = 0.95f,
            Cost = 1.4f,
            AllowedCharacters = new HashSet<RelativeId> { Hp2BaseMod.Girls.Jessie },
            Build = (seq, c) =>
            {
                // deeper emotional sink than base version
                seq.Append(c.Transform.DOLocalMoveY(
                    c.Transform.localPosition.y - 18f, 1.3f));

                // extended pause (resignation)
                seq.AppendInterval(1.8f);

                // weak correction attempt (barely recovers)
                seq.Append(c.Transform.DOLocalMoveY(
                    c.Transform.localPosition.y - 6f, 1.6f));

                seq.Join(c.Transform.DOScale(0.97f, 1.6f));

                // faint instability tremor
                seq.Append(c.Transform.DOShakePosition(0.7f, 5f, 12));
            }
        });

        // -------------------------
        // Jewn
        // -------------------------
        _animations.Add(new CharmAnimationDefinition
        {
            Weight = 1.2f,
            Cost = 1.4f,
            AllowedCharacters = new HashSet<RelativeId> { Hp2BaseMod.Girls.Jewn },
            Build = (seq, c) =>
            {
                seq.Append(c.Transform.DOScale(1.15f, 0.25f));
                seq.Append(c.Transform.DOLocalMoveX(c.Transform.localPosition.x + (25f * c.Dir), 0.25f));
                seq.Append(c.Transform.DOShakeAnchorPos(1.0f, 18f, 12));
                seq.Append(c.Transform.DOLocalMoveX(c.Transform.localPosition.x, 0.4f));
            }
        });

        _animations.Add(new CharmAnimationDefinition
        {
            Weight = 1.15f,
            Cost = 1.5f,
            AllowedCharacters = new HashSet<RelativeId> { Hp2BaseMod.Girls.Jewn },
            Build = (seq, c) =>
            {
                // superior judgment lean back (refusal posture)
                seq.Append(c.Transform.DOLocalMoveX(
                    c.Transform.localPosition.x - (12f * c.Dir), 0.3f));

                seq.AppendInterval(0.6f);

                // aggressive reassertion forward
                seq.Append(c.Transform.DOLocalMoveX(
                    c.Transform.localPosition.x + (22f * c.Dir), 0.25f));

                seq.Join(c.Transform.DOScale(1.18f, 0.25f));

                // cosmic instability shake (power leakage)
                seq.Append(c.Transform.DOShakeAnchorPos(0.9f, 16f, 14));

                // final snap back to dominance stance
                seq.Append(c.Transform.DOLocalMoveX(c.Transform.localPosition.x, 0.4f));
            }
        });

        // -------------------------
        // Kyu
        // -------------------------
        _animations.Add(new CharmAnimationDefinition
        {
            Weight = 1.1f,
            Cost = 0.8f,
            AllowedCharacters = new HashSet<RelativeId> { Hp2BaseMod.Girls.Kyu },
            Build = (seq, c) =>
            {
                seq.Append(c.Transform.DOLocalMoveX(c.Transform.localPosition.x + (18f * c.Dir), 0.3f));
                seq.Join(c.Transform.DOScale(1.08f, 0.3f));
                seq.Append(c.Transform.DOShakeAnchorPos(0.6f, 12f, 10));
                seq.Append(c.Transform.DOLocalMoveX(c.Transform.localPosition.x, 0.4f));
            }
        });

        _animations.Add(new CharmAnimationDefinition
        {
            Weight = 1.1f,
            Cost = 0.8f,
            AllowedCharacters = new HashSet<RelativeId> { Hp2BaseMod.Girls.Kyu },
            Build = (seq, c) =>
            {
                // quick interrupt hop (explaining something too fast)
                seq.Append(c.Transform.DOScale(1.1f, 0.2f));
                seq.Join(c.Transform.DOLocalMoveY(
                    c.Transform.localPosition.y + 6f, 0.2f));

                // sideways “oops correction”
                seq.Append(c.Transform.DOLocalMoveX(
                    c.Transform.localPosition.x + (14f * c.Dir), 0.25f));

                seq.Append(c.Transform.DOLocalMoveX(c.Transform.localPosition.x, 0.3f));

                // energetic settle wiggle
                seq.Append(c.Transform.DOShakeAnchorPos(0.5f, 8f, 10));
            }
        });

        // -------------------------
        // Lailani
        // -------------------------
        _animations.Add(new CharmAnimationDefinition
        {
            Weight = 0.9f,
            Cost = 0.9f,
            AllowedCharacters = new HashSet<RelativeId> { Hp2BaseMod.Girls.Lailani },
            Build = (seq, c) =>
            {
                seq.Append(c.Transform.DOLocalMoveX(c.Transform.localPosition.x + (10f * c.Dir), 0.3f));
                seq.Append(c.Transform.DOLocalMoveX(c.Transform.localPosition.x, 0.4f));
                seq.Append(c.Transform.DOShakeAnchorPos(0.8f, 7f, 14));
                seq.Append(c.Transform.DOScale(1f, 0.8f));
            }
        });

        _animations.Add(new CharmAnimationDefinition
        {
            Weight = 0.85f,
            Cost = 1.0f,
            AllowedCharacters = new HashSet<RelativeId> { Hp2BaseMod.Girls.Lailani },
            Build = (seq, c) =>
            {
                // sudden micro panic step back
                seq.Append(c.Transform.DOLocalMoveX(
                    c.Transform.localPosition.x - (14f * c.Dir), 0.25f));

                // pause (avoiding eye contact equivalent)
                seq.AppendInterval(0.7f);

                // cautious return
                seq.Append(c.Transform.DOLocalMoveX(
                    c.Transform.localPosition.x, 0.5f));

                // insecurity shake
                seq.Append(c.Transform.DOShakeAnchorPos(0.7f, 6f, 14));

                // slight shrink (self-doubt compression)
                seq.Append(c.Transform.DOScale(0.97f, 0.6f));
            }
        });

        // -------------------------
        // Lola
        // -------------------------
        _animations.Add(new CharmAnimationDefinition
        {
            Weight = 1.2f,
            Cost = 1.1f,
            AllowedCharacters = new HashSet<RelativeId> { Hp2BaseMod.Girls.Lola },
            Build = (seq, c) =>
            {
                seq.Append(c.Transform.DOLocalMoveY(c.Transform.localPosition.y + 8f, 0.2f));
                seq.Join(c.Transform.DOScale(1.1f, 0.2f));
                seq.Append(c.Transform.DOShakeAnchorPos(0.7f, 10f, 12));
                seq.Append(c.Transform.DOLocalMoveY(c.Transform.localPosition.y, 0.4f));
            }
        });

        _animations.Add(new CharmAnimationDefinition
        {
            Weight = 1.15f,
            Cost = 1.1f,
            AllowedCharacters = new HashSet<RelativeId> { Hp2BaseMod.Girls.Lola },
            Build = (seq, c) =>
            {
                // sharp productivity surge
                seq.Append(c.Transform.DOLocalMoveY(
                    c.Transform.localPosition.y + 10f, 0.18f));

                seq.Join(c.Transform.DOScale(1.12f, 0.18f));

                // fast correction (tight control behavior)
                seq.Append(c.Transform.DOLocalMoveY(
                    c.Transform.localPosition.y, 0.3f));

                // stress jitter (work overload)
                seq.Append(c.Transform.DOShakeAnchorPos(0.8f, 12f, 12));

                // final composure reset
                seq.Append(c.Transform.DOScale(1f, 0.4f));
            }
        });

        // -------------------------
        // Moxie
        // -------------------------
        _animations.Add(new CharmAnimationDefinition
        {
            Weight = 1.25f,
            Cost = 1.5f,
            AllowedCharacters = new HashSet<RelativeId> { Hp2BaseMod.Girls.Moxie },
            Build = (seq, c) =>
            {
                seq.Append(c.Transform.DOScale(1.2f, 0.2f));
                seq.Append(c.Transform.DOLocalMoveX(c.Transform.localPosition.x + (28f * c.Dir), 0.2f));
                seq.Append(c.Transform.DOShakeAnchorPos(1.1f, 20f, 14));
                seq.Append(c.Transform.DOLocalMoveX(c.Transform.localPosition.x, 0.4f));
            }
        });

        _animations.Add(new CharmAnimationDefinition
        {
            Weight = 1.3f,
            Cost = 1.6f,
            AllowedCharacters = new HashSet<RelativeId> { Hp2BaseMod.Girls.Moxie },
            Build = (seq, c) =>
            {
                // violent forward assertion
                seq.Append(c.Transform.DOLocalMoveX(
                    c.Transform.localPosition.x + (35f * c.Dir), 0.22f));

                seq.Join(c.Transform.DOScale(1.25f, 0.22f));

                // destabilizing shockwave
                seq.Append(c.Transform.DOShakeAnchorPos(1.2f, 22f, 16));

                // recoil suppression
                seq.Append(c.Transform.DOLocalMoveX(c.Transform.localPosition.x, 0.5f));

                // lingering power vibration
                seq.Append(c.Transform.DOShakePosition(0.6f, 10f, 10));
            }
        });

        // -------------------------
        // Nora
        // -------------------------
        _animations.Add(new CharmAnimationDefinition
        {
            Weight = 1.05f,
            Cost = 1.3f,
            AllowedCharacters = new HashSet<RelativeId> { Hp2BaseMod.Girls.Nora },
            Build = (seq, c) =>
            {
                seq.Append(c.Transform.DOLocalMoveX(c.Transform.localPosition.x + (16f * c.Dir), 0.35f));
                seq.AppendInterval(1.1f);
                seq.Append(c.Transform.DOLocalMoveX(c.Transform.localPosition.x, 0.55f));
                seq.Append(c.Transform.DOShakeAnchorPos(0.6f, 6f, 10));
            }
        });

        _animations.Add(new CharmAnimationDefinition
        {
            Weight = 1.0f,
            Cost = 1.3f,
            AllowedCharacters = new HashSet<RelativeId> { Hp2BaseMod.Girls.Nora },
            Build = (seq, c) =>
            {
                // restrained step forward
                seq.Append(c.Transform.DOLocalMoveX(
                    c.Transform.localPosition.x + (14f * c.Dir), 0.4f));

                // forced composure hold
                seq.AppendInterval(1.0f);

                // slight retreat (control reassertion)
                seq.Append(c.Transform.DOLocalMoveX(
                    c.Transform.localPosition.x, 0.55f));

                // suppressed instability leak
                seq.Append(c.Transform.DOShakeAnchorPos(0.5f, 5f, 10));

                // final stabilization micro-adjustment
                seq.Append(c.Transform.DOScale(1f, 0.3f));
            }
        });

        // -------------------------
        // Polly
        // -------------------------
        _animations.Add(new CharmAnimationDefinition
        {
            Weight = 1.2f,
            Cost = 1.0f,
            AllowedCharacters = new HashSet<RelativeId> { Hp2BaseMod.Girls.Polly },
            Build = (seq, c) =>
            {
                seq.Append(c.Transform.DOScale(1.1f, 0.3f));
                seq.Join(c.Transform.DOLocalMoveY(c.Transform.localPosition.y + 12f, 0.3f));
                seq.AppendInterval(0.9f);
                seq.Append(c.Transform.DOScale(1f, 0.5f));
                seq.Join(c.Transform.DOLocalMoveY(c.Transform.localPosition.y, 0.5f));
            }
        });

        _animations.Add(new CharmAnimationDefinition
        {
            Weight = 1.15f,
            Cost = 1.0f,
            AllowedCharacters = new HashSet<RelativeId> { Hp2BaseMod.Girls.Polly },
            Build = (seq, c) =>
            {
                // hopeful lift
                seq.Append(c.Transform.DOLocalMoveY(
                    c.Transform.localPosition.y + 14f, 0.25f));

                seq.Join(c.Transform.DOScale(1.12f, 0.25f));

                // dreamy pause
                seq.AppendInterval(1.0f);

                // gentle fall back
                seq.Append(c.Transform.DOLocalMoveY(
                    c.Transform.localPosition.y, 0.45f));

                seq.Join(c.Transform.DOScale(1f, 0.45f));

                // soft emotional aftershock
                seq.Append(c.Transform.DOShakeAnchorPos(0.4f, 7f, 10));
            }
        });

        // -------------------------
        // Zoey
        // -------------------------
        _animations.Add(new CharmAnimationDefinition
        {
            Weight = 1.25f,
            Cost = 0.9f,
            AllowedCharacters = new HashSet<RelativeId> { Hp2BaseMod.Girls.Zoey },
            Build = (seq, c) =>
            {
                seq.Append(c.Transform.DOScale(1.18f, 0.15f));
                seq.Append(c.Transform.DOScale(0.92f, 0.15f));
                seq.Append(c.Transform.DOScale(1.12f, 0.2f));
                seq.Append(c.Transform.DOShakeAnchorPos(0.9f, 13f, 18));
                seq.Append(c.Transform.DOLocalMoveY(c.Transform.localPosition.y + 9f, 0.35f));
                seq.Append(c.Transform.DOLocalMoveY(c.Transform.localPosition.y, 0.45f));
            }
        });

        _animations.Add(new CharmAnimationDefinition
        {
            Weight = 1.2f,
            Cost = 0.9f,
            AllowedCharacters = new HashSet<RelativeId> { Hp2BaseMod.Girls.Zoey },
            Build = (seq, c) =>
            {
                // perception instability oscillation
                seq.Append(c.Transform.DOScale(1.2f, 0.12f));
                seq.Append(c.Transform.DOScale(0.9f, 0.12f));
                seq.Append(c.Transform.DOScale(1.15f, 0.18f));

                // vertical drift (detached cognition)
                seq.Append(c.Transform.DOLocalMoveY(
                    c.Transform.localPosition.y + 12f, 0.3f));

                seq.Append(c.Transform.DOLocalMoveY(c.Transform.localPosition.y, 0.35f));

                // reality jitter burst
                seq.Append(c.Transform.DOShakeAnchorPos(1.0f, 15f, 18));
            }
        });
    }
}