# HuniePop 2 — Baggage & Class Design Notes

---

## Game Mechanics Summary

HuniePop 2 is a match-3 game where the goal is to earn a target amount of affection points within a limited move count.

### Core Resources

**Affection** is the scoring currency. It is earned by matching affection tokens on the board. There are four affection token types. Each character has a most favorite type (earns bonus affection) and a least favorite type (earns reduced affection). The affection goal must be met before moves run out to win the date.

**Passion** is a per-character multiplier applied to all affection that character earns. It is built by matching passion tokens. Higher passion means every affection match is worth more. Maxing passion before scoring is the default priority in most dates.

**Stamina** is the secondary cost resource. Every move made on a focused character drains her stamina. 3 tokens cost 1 stamina, 4 tokens cost 2, and 5+ cost 3. When stamina hits zero the character is exhausted and cannot be focused until their stamina is full again. The unfocused character passively regenerates stamina while moves on the other character are played. Stamina tokens on the board restore stamina to the focused character when matched.

**Sentiment** is the gift currency. It is accumulated by matching sentiment tokens. Sentiment is spent to purchase and use date gifts, which have various board effects such as clearing token types, boosting stats, or altering rules for the remainder of the date.

**Moves** are the hard limit. Each match costs one move from the counter. Joy tokens grant extra moves when matched. Running out of moves ends the date in failure if the affection goal has not been met.

**Power tokens** power tokens are upgraded affection tokens and have a chance to spawn when a match of 4 or more tokens is made. They are worth significantly more affection than standard matches and are the highest single-move affection source in the game.

### Focus

At any time the player is focused on one of the two characters. Matches are made on the focused character's side of the board. Making a move costs stamina from the focused character and regenerates stamina on the unfocused character. The player can switch focus at any time.

### Bad States

A character becomes **exhausted** when her stamina hits zero. In a two-character date this forces a focus switch to the other character until she recovers. A character becomes **upset** when a matching broken heart tokens. While upset she looses all stamina and cannot be focused until her stamina is full again.

### Single Date Mode

In single date mode there is only one character and she is always focused. There is no second character to switch to. There are no stamina tokens and characters do not have stamina. Matching broken hearts gives negative affection points instead of making characters upset.

---

## Single Date vs Double Date

| | Double Date | Single Date |
|---|---|---|
| Focus | Switches between two characters | Always on the same character |
| Upset consequence | Focused character looses all stamina and becomes exhausted | Negative affection points |
| Stamina | Cost resource that limits large moves and requires the player to switch focus | N/A |
| Exhaustion consequence | Forced focus switch, cannot focus until stamina is full | N/A |

### Design Considerations for Baggage on Solo dates

- Characters should only have at most 1 baggage that does not apply to single dates (focus, exhaust, upset)
- No character should be unable to win on their own

---

## Character Classes

Classes are defined by what a character's baggage makes them good at. Most characters will fit one primary class and may lean toward a secondary. Knowing a character's class determines the optimal pairing and playstyle.

### Scorer
The primary affection earner. Designed to be the focus during high-passion scoring runs. Baggage amplifies affection output, rewards big matches, or creates high-payoff conditions. The standard strategy builds the Scorer's passion first and then spends the remaining moves scoring on them as efficiently as possible.

### Support
The sentiment and stamina engine. Designed to fund gifts and sustain the other character's scoring. Baggage rewards generating sentiment, sustaining stamina, or redirecting resources to the date. Their affection output is low but their enabling value is high.

### Catalyst
Passion engine. Keeps both characters' multipliers high simultaneously, enabling dual-scorer compositions that bypass the standard one-scores-one-supports model. Their own affection output is low; their value is entirely in the multiplier they provide.

### Detonator
Burst scorer. Suppresses output for most of the date in exchange for one large payoff moment. Requires setup time and a supporting partner to cover the dead phase. Rewards patience and precise timing.

### Anchor
Stamina tank. Allows extended focus runs without exhaustion threatening a forced switch. Enables long setup phases for big matches. Output per move is modest but output per stamina point is excellent.

### Disruptor
Board manipulator. Warps the token economy as a side effect of her baggage, creating board states that would be impossible to engineer manually. Does not score or support directly — instead creates openings for a partner to exploit.

### Miser
Sentiment hoarder. Treats sentiment as a scoring amplifier rather than a currency to spend. Rewards letting sentiment accumulate to high levels and punishes gift usage. Pairs well with characters who can win without frequent gifts. Affects should typically affect the other character, since the miser isn't spending sentiment anyways

### Pendulum
Focus swapper. Rewards alternating focus frequently rather than exhausting one character before switching. Every return to her triggers a bonus, and leaving her for too long carries a penalty. Enables a back-and-forth rhythm neither character in the default strategy ever develops.

### Monk
Restraint player. Gains power through deliberate avoidance — not matching her least favorite type, not using gifts, keeping stamina above a threshold. Rewards discipline and punishes greedy play.

### Wildcard
High-variance scorer. Output swings unpredictably. Capable of exceeding any other class's ceiling but equally capable of collapsing entirely. Requires a stable partner to absorb the bad swings. Rewards risk tolerance and board awareness.

### Mirror
Reaction player. Her output scales directly off her date's visible stats — passion level, stamina level, gift slots filled. Nearly useless in solo. Strongest when paired with a fully invested Scorer or Catalyst.

## Base Game Baggage
 
### Lola
*Best classes: Scorer, Anchor*
 
Caffeine Junkie makes Lola almost entirely self-sufficient on stamina — she absorbs stamina tokens regardless of who matches them, making stamina management trivially easy while she is focused. Busy Schedule punishes long dates, so she rewards fast aggressive scoring. Miss Independent is near-irrelevant because the Support will be gifting Lola, and any date where gifts matter will already have enough passion built by the time sentiment is spent.
 
**Ideal pairings:** Any Support or Catalyst. Lola is a clean Scorer with minimal caveats — she benefits from a date who generates sentiment quickly so gifts can be spent on her early. Avoid pairing with characters who also need stamina tokens (e.g. another Anchor) since Caffeine Junkie starves the date of stamina token recovery.
 
**Low impact baggage:** Miss Independent. A 30% passion threshold for receiving gifts is irrelevant in any sensible build — gifts from the Support arrive after passion is established anyway.
 
*Thematic replacement suggestion:* Miss Independent could instead make Lola refuse gifts unless she has more affection than her date at the moment of gifting — preserving the independent spirit thematically while creating a real constraint that requires planning around gift timing.
 
| Name | Description | Pros | Cons | Notes |
| --- | --- | --- | --- | --- |
| Busy Schedule | Lola will shorten the date due to time constraints by taking an additional 1 Move after every 10 moves. | N/A | Fewer moves over a long date | Punishes slow or drawn-out dates. Pairs badly with Detonator playstyles that need time to build. Rewards fast aggressive scoring. |
| Caffeine Junkie | Lola's caffeine addiction will cause her to absorb all Stamina matches, unless she's exhausted or upset. | Lola gains stamina from any stamina match regardless of who makes it | Her date cannot gain stamina from token matches at all; must rely on passive regen | Strong Anchor/Scorer trait. Deliberately exhausting or upsetting Lola temporarily frees stamina tokens for her date. |
| Miss Independent | Lola will refuse to accept date gifts unless she has at least 30% Passion. | N/A | Gifts are gated behind a passion threshold | Near-irrelevant. Gifts come from the Support's sentiment and are given to Lola — by the time there is enough sentiment to spend, passion is already built. Only matters in fringe early-gift strategies. |
 
---
 
### Jessie
*Best classes: Support*
 
Depression drains sentiment while Jessie is focused, which hurts her ability to fund gifts for her date. Since sentiment flows from Jessie to her Scorer, this is a meaningful penalty — she cannot hold sentiment in reserve and must spend it quickly. Emphysema makes her stamina recovery unreliable whether she is focused or unfocused, making her fragile as either role. Busted Vadge locks Sexuality as a scoring token unless large matches are made, which is irrelevant if Jessie is playing Support and not scoring directly.
 
**Ideal pairings:** A Scorer with low gift dependency. Because Depression bleeds sentiment, Jessie is best with a date who needs few gifts or prefers immediate-effect gifts over passive ones — so there is nothing to lose to the drain. Avoid Miser or Monk builds that want to sit on high sentiment.
 
**Low impact baggage:** Busted Vadge. As a Support, Jessie is not scoring on Sexuality tokens, so the restriction is largely irrelevant. Only matters if Jessie is ever used as a Scorer.
 
*Thematic replacement suggestion:* Busted Vadge could instead cause Jessie to shed 1 Stamina whenever a Sexuality match is made on her side regardless of size — keeping the thematic idea of sensitivity without being a dead baggage in Support builds.
 
| Name | Description | Pros | Cons | Notes |
| --- | --- | --- | --- | --- |
| Depression | A deep depression will cause Jessie to shed 1 Sentiment after every 5 moves. | N/A | Sentiment bleeds away; cannot fund gifts reliably over a long date | Sentiment earned by Jessie funds gifts for her Scorer date. The drain means gifts must be spent quickly or they are wasted. Encourages frequent small gifts over saving for expensive ones. |
| Emphysema | There's a 20% chance that Jessie will fail to recover Stamina when a move is made on her date. | N/A | Stamina recovery while unfocused is unreliable | Passive regen is the Support's main stamina source. The 20% failure rate is a consistent drag. Stamina token matches while focused become more important. |
| Busted Vadge | Jessie won't feel Sexuality matches at all unless they are at least 4-of-a-kind+. | N/A | Standard Sexuality matches yield no affection | Low impact in Support builds since Jessie is not the affection earner. Only relevant if Sexuality is her favorite type and she is being used as a Scorer. |
 
---
 
### Lillian
*Best classes: Support, Catalyst*
 
The Darkness is a passive positive — broken hearts become useful and passion tokens on her side become harmful to avoid, which is easy to manage. Teen Angst makes joy tokens a board hazard while focused on Lillian, which is painful for a Support who benefits from extra moves. Asthma punishes large matches, which keeps her away from Scorer builds but is fine for Support who prefers small frequent moves.
 
**Ideal pairings:** Nora. Emotionally Guarded doubles broken heart token spawns across the whole board — Lillian's The Darkness converts those broken hearts into useful tokens for her, turning Nora's worst board hazard into Lillian's resource. This is the strongest thematic and mechanical pairing in the base game. Also pairs well with Sarah, whose Annoying as Fuck spawns broken hearts on matches — again converted to Lillian's benefit.
 
**Low impact baggage:** None. All three of Lillian's baggages are felt consistently. Teen Angst and Asthma both have clear mechanical weight. The Darkness is purely positive but always active.
 
| Name | Description | Pros | Cons | Notes |
| --- | --- | --- | --- | --- |
| The Darkness | The effects of Passion tokens and Broken Heart tokens are reversed for Lillian. | Broken hearts become beneficial; clears negative token clutter passively | Passion tokens on her side are now harmful — must be avoided or cleared | Exclusively positive in most contexts. Synergises strongly with any baggage that generates broken hearts (Nora's Emotionally Guarded, Sarah's Annoying as Fuck). |
| Asthma | Moves on Lillian that include multiple matches or a 4-of-a-kind+ match will cause her to lose an additional 1 Stamina. | N/A | Big moves are stamina-expensive; power token plays are risky | Punishes the exact board states that generate power tokens. Pushes her toward small, frequent matches. Fine as Support. Poor Scorer. |
| Teen Angst | Joy matches directed at Lillian will subtract Moves instead of adding them. | N/A | Joy tokens are actively harmful while focused on her | Avoid matching joy tokens while focused on Lillian. Her date's joy tokens are unaffected. Significant move economy penalty on long focus runs. |
 
---
 
### Zoey
*Best classes: Disruptor, Wildcard*
 
Kinda Crazy makes the board constantly refresh, which punishes setup-heavy play and rewards reacting to whatever appears. Aquaphobic inverts sentiment and broken hearts — because sentiment earned by Zoey funds gifts for her date, Aquaphobic means Zoey must match broken hearts (normally bad) to generate that currency, which combined with Kinda Crazy's board chaos makes her genuinely difficult to pilot. Tinnitus removes joy tokens while focused, cutting off the move economy entirely during her turns.
 
**Ideal pairings:** A stable Scorer with low gift dependency. Zoey is chaotic to focus on, so her date should be able to score independently without needing many gifts from Zoey's sentiment. Ashley (Commitment Issues) is an interesting match — both characters demand frequent focus switching, so Zoey's Tinnitus becomes less punishing since you switch away before joy tokens are needed anyway.
 
**Low impact baggage:** None — all three are consistently felt. Kinda Crazy in particular is one of the most impactful baggages in the whole game.
 
| Name | Description | Pros | Cons | Notes |
| --- | --- | --- | --- | --- |
| Kinda Crazy | A random row of tokens on the grid will shatter every time a move is made on Zoey. | Constantly refreshes the board; can break bad rows by accident | Board state is impossible to plan around; setups collapse immediately | The most chaotic base baggage. Reactive play only. Punishes Detonator and Monk builds that need a stable board. |
| Aquaphobic | The effects of Sentiment tokens and Broken Heart tokens are reversed for Zoey. | Broken heart tokens generate sentiment; normally negative tokens become her currency | Sentiment tokens function as broken hearts; normal sentiment generation harms her | Zoey must match broken hearts to fund gifts for her date. Sentiment tokens on her side must be avoided. Completely inverts Support priorities. |
| Tinnitus | A constant ringing in Zoey's ears will prevent any Joy tokens from falling while your focus is on her. | Simplifies the board slightly by removing one token type | No move bonuses available during Zoey's focus runs | Severe move economy penalty. Switch focus to earn joy tokens then return. Pairs badly with any build that needs sustained focus on Zoey. |
 
---
 
### Sarah
*Best classes: None (managed liability)*
 
All three of Sarah's baggages are pure negatives with no mechanical upside. Annoying as Fuck clutters the board with broken hearts, Attention Whore randomly wastes moves and breaks combos, and Smelly Pussy reduces passion token frequency for the whole date with no compensation. The only partial silver lining is that Attention Whore and Smelly Pussy are both disabled when Sarah is exhausted or upset, creating a rare case where keeping a character in a bad state is actively beneficial for a sustained period.
 
**Ideal pairings:** Lillian. The Darkness converts Sarah's broken heart clutter (Annoying as Fuck) into a resource for Lillian, making the most persistent negative into a passive positive for her date. Any Scorer who can build passion quickly also mitigates Smelly Pussy's impact by front-loading passion before the reduced token rate becomes a bottleneck.
 
**Low impact baggage:** None — all three are consistently punishing. Smelly Pussy is arguably the most impactful because it affects the entire date for both characters with no counterplay other than playing faster.
 
*Thematic replacement suggestion:* Smelly Pussy reduces passion tokens by 20% with no upside. A thematically adjacent replacement would use Repellent from our brainstorming — reducing all affection gained on her side by a flat amount until a gift is used on her, rewarding player responsiveness rather than just applying a passive tax.
 
| Name | Description | Pros | Cons | Notes |
| --- | --- | --- | --- | --- |
| Annoying as Fuck | There's a 15% chance that Sarah will leave behind a Broken Heart token whenever a match is directed at her. | N/A | Board gradually fills with broken heart tokens | Compounds over time. Synergises destructively with Emotionally Guarded. Pairs well against Lillian whose Darkness converts them. |
| Attention Whore | There's a 15% chance that Sarah will steal your focus away from her date right before a move is made, unless she's exhausted or upset. | Exhausting or upsetting her permanently disables the effect | Unpredictable focus switches waste moves and break setups | Makes keeping Sarah exhausted or upset a valid deliberate strategy. One of the few cases in the base game where bad states are actively sought. |
| Smelly Pussy | A fishy aroma emanating from Sarah will reduce the amount of Passion tokens that fall by 20% for the duration of the date. | N/A | Passion building is slower for both characters the entire date | No counterplay other than building passion aggressively early. Catalysts suffer most. A passive whole-date tax with no upside. |
 
---
 
### Lailani
*Best classes: Monk, Scorer (Romance-specialist)*
 
Old Fashioned effectively removes Sexuality for the first 15 moves, which is manageable with board awareness. Low Self Esteem is the most interesting — it makes power token matches worthless unless the match is Romance type, which means designating Romance as her favorite token type completely negates the downside and turns her into a strong focused Scorer. Sheepish restricts two gift categories but is rarely impactful since the Support gifts from their own sentiment pool.
 
**Ideal pairings:** A Support who generates sentiment quickly so Lailani can receive gifts early, helping her past the Old Fashioned window. If running Lailani as a Romance-specialist Scorer, pair with any reliable sentiment-generating Support. Avoid pairings where the Support also has restricted gift categories — stacking gift restrictions across both characters limits options severely.
 
**Low impact baggage:** Sheepish. Since gifts are funded by the Support's sentiment and given to Lailani, the restriction only matters if Hygiene or Sex Toy gifts were the planned strategy. Rarely meaningful.
 
*Thematic replacement suggestion:* Sheepish could instead become Low Confidence — Lailani refuses to allow any gifts at all until her passion is above 50%, making the early game gift-free and forcing the Support to hold sentiment until the threshold is reached. More impactful and thematically consistent.
 
| Name | Description | Pros | Cons | Notes |
| --- | --- | --- | --- | --- |
| Old Fashioned | For the first 15 moves of the date, Sexuality matches directed at Lailani will cause her to become upset. | Clears itself after move 15; a temporary constraint | Sexuality off-limits early; accidental matches trigger upset | Manageable with board awareness. Critical if Sexuality is her favorite type. Gets worse with low move counts since 15 moves is a higher proportion of the date. |
| Low Self Esteem | Affection matches directed at Lailani that include a Power token won't yield any Affection, unless it's a Romance match. | Making Romance her favorite type completely negates this baggage | Power token matches on non-Romance types are entirely wasted | Effectively removes power tokens for most Scorer builds. A non-issue if Romance is designated her favorite. The single biggest factor in deciding her token type assignment. |
| Sheepish | Lailani won't allow any Hygiene or Sex Toy date gifts, unless she's exhausted or upset, because it's too embarrassing. | Exhausting or upsetting her unlocks these gifts | Two gift categories unavailable | Low impact. Gifts come from the Support's sentiment pool — this only matters if Hygiene or Sex Toy gifts were specifically planned. |
 
---
 
### Candace
*Best classes: Anchor, Detonator*
 
Intellectually Challenged removes the stamina penalty from large matches and replaces it with a move cost — a direct tradeoff between time and stamina. This makes her surprisingly durable for extended large-match play, at the cost of a shrinking move counter. Forgetful makes passive gifts unreliable for both characters, which is a penalty on the Support's sentiment investment as much as on Candace herself. Hypersensitive makes upset potentially catastrophic — 30% passion loss on both characters is severe enough to end a date if it happens late.
 
**Ideal pairings:** A Support with high stamina tolerance and preference for immediate-effect gifts over passive ones, to mitigate Forgetful. Avoid pairings with characters whose baggage increases upset risk (e.g. Hair Trigger builds, or characters with negative token penalties that could cascade into upset). A Disruptor pairing is interesting — Candace's Intellectually Challenged lets her chain large matches without stamina fear while a Disruptor loads the board.
 
**Low impact baggage:** None — all three are consistently felt. Forgetful's 5% per move accumulates meaningfully over a full date.
 
| Name | Description | Pros | Cons | Notes |
| --- | --- | --- | --- | --- |
| Intellectually Challenged | Big moves on Candace won't use extra Stamina, but will cost an extra Move for her to figure out. | Large matches and power token plays cost no extra stamina | Each big move costs an extra move from the counter | Strong Anchor trait. Enables chaining large matches without exhaustion risk. The date gets shorter, but Candace stays fresh. Best paired with a move-efficient Support. |
| Forgetful | There's a 5% chance per move that Candace will forget about a date gift that either she or her date have received, removing its passive effects. | N/A | Passive gift effects can disappear at any time for both characters | Affects gifts given to both Candace and her date. Prefer immediate-effect gifts. Low probability per move but significant over a full date. Makes the Support's sentiment investment unreliable. |
| Hypersensitive | If Candace becomes upset she'll drain 30% Passion from both herself and her date. | N/A | Upset causes massive passion loss for both characters | The most punishing upset consequence in the base game. Keeping Candace calm is non-negotiable. Avoid her least favorite type rigorously. Pairs very badly with anything that increases upset risk. |
 
---
 
### Nora
*Best classes: Scorer (stay-focused), Anchor*
 
Abandonment Issues makes every focus switch away from Nora cost 5% of current total affection — which compounds heavily as affection accumulates. This pushes Nora toward an extreme stay-focused Scorer build where focus is never voluntarily switched. Emotionally Guarded floods the board with broken hearts that only harm her date, not her. Vindictive makes upset catastrophic for both characters' gift investments. Together these make Nora one of the most demanding characters in the base game.
 
**Ideal pairings:** Lillian. Emotionally Guarded doubles broken heart spawns across the board — Lillian's The Darkness converts all of those into positive tokens for her, turning Nora's worst passive into Lillian's resource. This is the single strongest synergy pairing in the base game. Lillian also never needs Nora to switch focus to her since Lillian's Support role works best while Nora stays focused scoring.
 
**Low impact baggage:** None — all three are severely impactful.
 
| Name | Description | Pros | Cons | Notes |
| --- | --- | --- | --- | --- |
| Abandonment Issues | Any time your focus is turned away from Nora, she'll drain 5% of the current earned Affection for the date. | N/A | Every focus switch costs a percentage of total affection | Compounds as the date progresses — a late-date switch is far more expensive than an early one. Nora must stay focused. Pairs best with an Anchor Scorer build or a Support who never needs focus switched to them. |
| Emotionally Guarded | Doubles the amount of Broken Heart tokens that fall, but Nora will be immune to Broken Heart matches. | Nora is completely immune to broken heart penalties | Her date suffers doubled broken heart frequency and takes full damage | The date bears all the broken heart burden. Pair with Lillian whose The Darkness converts them. Otherwise plan on frequent board-clear gifts. |
| Vindictive | If Nora becomes upset she'll throw away one date gift each that she and her date have received, removing their passive effect. | N/A | Upset destroys gift investment for both characters simultaneously | Prefer immediate-effect gifts to minimise loss. Keeping Nora calm is as critical as Hypersensitive on Candace. The Support's sentiment investment is at risk every time Nora might be upset. |
 
---
 
### Brooke
*Best classes: Scorer, Miser*
 
Unsentimental reduces sentiment token frequency for the whole date, which hurts the Support's ability to fund gifts for Brooke. Gold Digger takes a move whenever her date receives a gift — directly taxing the Support's primary action. Together these make Brooke hostile to the standard support-gift loop. She rewards builds that win with few or no gifts, or strategies that exhaust/upset her before gifting the date to disable Gold Digger. Expensive Tastes is low impact.
 
**Ideal pairings:** A Scorer or Monk build as her date who does not need many gifts. Brooke is best when gift usage is minimised — her baggage punishes both the generation and delivery of gifts. A Miser pairing where neither character relies on gifts is strong. Avoid pairing with characters who need frequent board-clearing gifts to manage their own baggage (e.g. Zoey or Sarah).
 
**Low impact baggage:** Expensive Tastes. Gift categories are a minor restriction that rarely affects the outcome unless Plushie or Souvenir gifts were specifically planned.
 
*Thematic replacement suggestion:* Expensive Tastes could be replaced with Gold Standard — Brooke refuses to allow any gift below a minimum sentiment cost threshold, forcing the Support to save for expensive gifts rather than spending small ones freely. More impactful and consistent with her Gold Digger personality.
 
| Name | Description | Pros | Cons | Notes |
| --- | --- | --- | --- | --- |
| Expensive Tastes | Brooke won't allow any Plushie or Souvenir date gifts, unless she's exhausted or upset, because she views them as cheap. | Exhausting or upsetting her unlocks these gift types | Two gift categories unavailable | Low impact unless those specific categories were planned. |
| Unsentimental | Brooke's unsentimentality will reduce the amount of Sentiment tokens that fall by 20% for the duration of the date. | More affection tokens, easier matches | Weakens support wether it's Brooke or her data. | Hurts the Support's ability to fund gifts for Brooke. The whole-date nature makes it a persistent drag. Front-load sentiment generation. |
| Gold Digger | Brooke will take 1 Move for herself any time her date receives a date gift, unless she's exhausted or upset. | Exhausting or upsetting Brooke disables the effect entirely | Gifting Brooke's date costs an extra move each time | Directly taxes the Support's gifting action. Exhaust or upset Brooke before gifting the date. Another base game example of bad states being deliberately useful. |
 
---
 
### Ashley
*Best classes: Pendulum*
 
Commitment Issues is the most class-defining baggage in the base game — it directly forces Pendulum playstyle by making staying focused a liability after 3 moves. Easily Bored adds a move cost to repeating token types, which combines with Commitment Issues to demand both variety in token type and frequency of focus switching simultaneously. Allergies is low impact.
 
**Ideal pairings:** Any character who benefits from frequent focus switches — a Pendulum-class date (Fresh Eyes, Absence Makes the Heart) turns the mandatory switching into a shared advantage for both characters. Nora is a poor pairing — Abandonment Issues punishes every switch away from Nora, directly conflicting with Ashley's Commitment Issues which demands switching away every 3 moves.
 
**Low impact baggage:** Allergies. Gift category restriction rarely determines outcomes.
 
*Thematic replacement suggestion:* Allergies could become Fickle — Ashley loses 10% passion whenever the same gift type is used twice on her during the date, preserving the allergy-to-repetition theme while creating a real constraint on gift strategy that complements Easily Bored.
 
| Name | Description | Pros | Cons | Notes |
| --- | --- | --- | --- | --- |
| Easily Bored | Two consecutive moves that include the same type of match will cost an extra Move. Moves with multiple matches excluded. | Rewards varied board reading and flexible matching | Repetitive matching is penalised; bad board states force expensive same-type runs | Compounds with Commitment Issues — both demand variety. Pairs well with Dead Ringer (all types equal) as that build avoids favourite-type concentration anyway. |
| Commitment Issues | If you stay focused on Ashley for 4 consecutive moves, she'll become uncomfortable and matches directed at her will be negative. | Forces focus switching which can benefit Pendulum-style date partners | Cannot stay focused for more than 3 consecutive moves under any circumstances | The most Pendulum-defining baggage in the base game. Mandatory switching every 3 moves. Pairs excellently with dates who benefit from frequent focus switches, and disastrously with Nora's Abandonment Issues. |
| Allergies | Ashley won't allow any Flower or Candle date gifts, unless she's exhausted or upset, because of her allergies. | Exhausting or upsetting her unlocks these gifts | Two gift categories unavailable | Low impact in most builds. |
 
---
 
### Abia
*Best classes: Scorer*
 
| Name | Description | Pros | Cons | Notes |
| --- | --- | --- | --- | --- |
| Sex Addict | Abia will absorb all Sexuality matches and she'll consume a random Sexuality token every time a move is made on her. | Ideal passion target and scorer. Date can spend stamina directly on her by matching sexuality. | Cripples date's scoring potential but Abia is the scorer anyways | Sexuality is Abia's exclusive resource. Her date must avoid Sexuality as a token type entirely. Strong stamina Anchor. |
| One Pump Chump | A 4-of-a-kind+ match directed at Abia will cause her to squirt out half of her Sentiment. Sentiment matches excluded. | N/A | Large matches drain half of current sentiment; power token plays are expensive | Makes Abia a worse support, but she wasn't a support anyways. |
| Self Effacing | Abia will refuse to accept date gifts unless her date has already received one before her. | N/A | Gift order is locked; date must be gifted first every time | Slower start. Abia is typically a scorer but must first gain enough sentiment to give a gift, |
 
### Polly
*Best classes: Scorer*
Her baggage is mostly inconsequential. Leans slightly towards scorer due to Jealousy, but that's it.
 
| Name | Description | Pros | Cons | Notes |
| --- | --- | --- | --- | --- |
| Drama Queen | If Polly becomes upset she'll drain 4 Sentiment from both herself and her date. | N/A | Upset is a significant sentiment penalty for both characters | Keeping Polly calm is high priority. 4 sentiment is a meaningful cost at any stage of the date. Pairs badly with any baggage that increases upset frequency. |
| Jealousy | Polly will drain 5% of the total Affection Goal for the date any time her date receives a date gift, unless she's exhausted or upset. | Exhausting or upsetting her disables the effect | Gifting the date costs a percentage of the affection goal each time | One of the most punishing gift-related baggages. Discourages gifting the date entirely. Exhausting Polly before gifting the date is a strong strategy. Pairs well with Nest Egg or any build that wins without gifts. |
| Brand Loyalist | Polly won't allow any Jewelry or Cosmetic date gifts, unless she's exhausted or upset, because she doesn't like off-brand products. | Exhausting or upsetting her unlocks these gifts | Two gift categories unavailable normally | Low impact unless the strategy depends on those categories. |

## Notes

Some baggages are disabled when upset/exhausted which tries to support strategy, but it practice it is never worth purposely making someone unset or exhaust so these factors only slightly lessen a bad situation. These states need to become less punishing so they can be better used for gameplay depth. Faster recovery time?



