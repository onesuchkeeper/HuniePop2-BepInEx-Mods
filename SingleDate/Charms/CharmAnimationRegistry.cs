using System.Collections.Generic;
using DG.Tweening;
using Hp2BaseMod;
using UnityEngine;
using System.Linq;
using HarmonyLib;
using System;

namespace SingleDate;

/// <summary>
/// The pool of idle animations for the charm shown on single dates, and the logic to string a random selection of
/// them together into one sequence.
/// </summary>
public class CharmAnimationRegistry
{
    // Every registered animation, the built in ones first.
    private readonly List<CharmAnimationDefinition> _animations = new();

    public CharmAnimationRegistry()
    {
        RegisterAll();
    }

    /// <summary>
    /// Builds a sequence of random animations that fits a time budget. Repeatedly picks a valid animation (weighted
    /// random, affordable with the remaining budget and allowed for the girl) and appends it, until the budget is spent.
    /// </summary>
    /// <param name="context">The charm, which animations read and may change the direction of.</param>
    /// <param name="budget">The total cost to spend. Costs are tuned by hand to roughly match animation lengths in seconds.</param>
    public Sequence CreateSequence(CharmAnimationContext context, float budget)
    {
        var seq = DOTween.Sequence();
        float remaining = budget;

        // Hard stop so a zero cost animation can't loop forever.
        int safety = 0;

        while (remaining > 0.01f && safety++ < 20)
        {
            var pool = GetValidAnimations(context.GirlId, remaining);

            if (pool.Count == 0)
            {
                // Nothing affordable is left, pad with a short pause and finish.
                seq.AppendInterval(0.5f);
                break;
            }

            var anim = WeightedPick(pool);

            remaining -= anim.Cost;

            anim.Build(seq, context);
        }

        return seq;
    }

    /// <summary>The animations that cost no more than the budget and are allowed for the girl.</summary>
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

    /// <summary>Picks randomly, with each animation's chance proportional to its weight.</summary>
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

    /// <summary>
    /// Adds an animation to the pool. Used by the interop method <see cref="Plugin.RegisterCharmAnimation"/>.
    /// If <paramref name="build"/> returns true the charm's direction is flipped afterwards.
    /// </summary>
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
        void AddAnimation(float weight, Action<Sequence, CharmAnimationContext> build, HashSet<RelativeId> allowedCharacters = null)
        {
            _animations.Add(new CharmAnimationDefinition
            {
                Weight = weight,
                Cost = 1,
                AllowedCharacters = allowedCharacters,
                Build = build
            });
        }
    
        // hop sideways a short random distance.
        AddAnimation(2f, (seq, c) =>
        {
            seq.Append(c.Transform.DOLocalJump(
                c.Transform.localPosition + new Vector3(UnityEngine.Random.Range(-60f, 60f), 0),
                UnityEngine.Random.Range(10f, 20f),
                UnityEngine.Random.Range(1, 2),
                1.2f).SetEase(Ease.Linear));
        });
    
        // flip the facing direction, no tween.
        AddAnimation(2f, (seq, c) =>
        {
            c.Dir *= -1;
        });
    
        // squash, joined to the previous tween.
        AddAnimation(1f, (seq, c) =>
        {
            seq.Join(c.Transform.DOScaleY(0.95f, 1f).SetEase(Ease.InOutElastic));
            seq.Join(c.Transform.DOScaleX(1.05f * c.Dir, 1f).SetEase(Ease.InOutElastic));
        });
    
        // stretch, joined to the previous tween.
        AddAnimation(1f, (seq, c) =>
        {
            seq.Join(c.Transform.DOScaleY(1.05f, 1f).SetEase(Ease.InOutElastic));
            seq.Join(c.Transform.DOScaleX(0.95f * c.Dir, 1.2f).SetEase(Ease.InOutElastic));
        });
    
        // flip around and back.
        AddAnimation(2f, (seq, c) =>
        {
            seq.Append(c.Transform.DOScaleX(-1f * c.Dir, 1).SetEase(Ease.InSine));
            seq.Append(c.Transform.DOScaleX(1f * c.Dir, 1).SetEase(Ease.OutSine));
        });
    
        // build up, big jump, and a squash + spin that plays alongside the jump.
        AddAnimation(1f, (seq, c) =>
        {
            // build up
            seq.Append(c.Transform.DOScaleX(1.4f * c.Dir, 1.2f).SetEase(Ease.InOutSine));
            seq.Join(c.Transform.DOScaleY(0.75f, 1.2f).SetEase(Ease.InOutSine));
            seq.AppendInterval(0.45f);
    
            // jump
            seq.Append(c.Transform.DOLocalJump(c.Transform.localPosition, 200f, 1, 3.5f).SetEase(Ease.OutBack));
    
            // spin
            var squashNSpin = DOTween.Sequence().SetLoops(1, LoopType.Incremental);
    
            squashNSpin.Append(c.Transform.DOScaleX(0.85f * c.Dir, 0.25f).SetEase(Ease.OutSine));
            squashNSpin.Join(c.Transform.DOScaleY(1.1f, 0.25f).SetEase(Ease.OutSine));
    
            squashNSpin.Append(c.Transform.DORotate(new Vector3(0f, 0f, c.Dir * 360f), 2.3f, RotateMode.FastBeyond360).SetEase(Ease.OutBack));
    
            squashNSpin.Append(c.Transform.DOScaleX(c.Dir, 1f).SetEase(Ease.OutElastic));
            squashNSpin.Join(c.Transform.DOScaleY(1f, 1f).SetEase(Ease.OutElastic));
    
            // The old code chained two OnComplete calls on squashNSpin (pivot 0.5,0.5 after the squash, then 0.5,0 after
            // the spin). Both set the sequence's own callback, so the second replaced the first and only this one ever ran.
            squashNSpin.OnComplete(() =>
            {
                c.Transform.pivot = new Vector2(0.5f, 0f);
            });
    
            seq.Join(squashNSpin);
        });

        // just wait.
        AddAnimation(5f, (seq, c) =>
        {
            seq.AppendInterval(2f);
        });
    
        // Sarah
        AddAnimation(1f, (seq, c) =>
        {
            seq.Append(c.Transform.DOScaleX(c.Dir * 1.3f, 0.4f).SetEase(Ease.OutElastic));
            seq.Join(c.Transform.DOScaleY(0.5f, 0.4f).SetEase(Ease.OutElastic));
    
            seq.Append(c.Transform.DOScaleX(c.Dir * 0.5f, 0.4f).SetEase(Ease.OutElastic));
            seq.Join(c.Transform.DOScaleY(1.3f, 0.4f).SetEase(Ease.OutElastic));
    
            seq.Append(c.Transform.DOScaleX(c.Dir * 1.3f, 0.4f).SetEase(Ease.OutElastic));
            seq.Join(c.Transform.DOScaleY(0.5f, 0.4f).SetEase(Ease.OutElastic));
    
            seq.Append(c.Transform.DOScaleX(c.Dir * 0.5f, 0.4f).SetEase(Ease.OutElastic));
            seq.Join(c.Transform.DOScaleY(1.3f, 0.4f).SetEase(Ease.OutElastic));
        }, new HashSet<RelativeId> { Hp2BaseMod.Girls.Sarah });
    
        // Lillian
        AddAnimation(1f, (seq, c) =>
        {
            seq.Append(c.Transform.DOLocalMoveX(c.Transform.localPosition.x + (90 * c.Dir), 1.2f));
            seq.Join(c.Transform.DOLocalMoveY(c.Transform.localPosition.y + 48, 1.2f));
            seq.Join(c.Transform.DOScaleY(0.9f, 1.2f));
            seq.Join(c.Transform.DOScaleX(1.1f * c.Dir, 1.2f));
            seq.Join(c.Transform.DORotate(new Vector3(0f, 0f, c.Dir * 90), 1.2f));
    
            seq.Append(c.Transform.DOScaleY(1.1f, 1.2f).SetEase(Ease.InOutElastic));
            seq.Join(c.Transform.DOScaleX(0.9f * c.Dir, 1.2f).SetEase(Ease.InOutElastic));
    
            seq.AppendInterval(UnityEngine.Random.Range(8f, 30f));
    
            seq.Append(c.Transform.DOScaleY(0.9f, 1.2f).SetEase(Ease.InOutSine));
            seq.Join(c.Transform.DOScaleX(1.1f * c.Dir, 1.2f).SetEase(Ease.InOutSine));
            seq.Join(c.Transform.DORotate(new Vector3(0f, 0f, 0), 1.2f));
            seq.Join(c.Transform.DOLocalMoveX(c.Transform.localPosition.x, 1.2f));
            seq.Join(c.Transform.DOLocalMoveY(c.Transform.localPosition.y, 1.2f));
    
            seq.Append(c.Transform.DOScaleY(1f, 1.2f).SetEase(Ease.InOutElastic));
            seq.Join(c.Transform.DOScaleX(1f * c.Dir, 1.2f).SetEase(Ease.InOutElastic));
        }, new HashSet<RelativeId> { Hp2BaseMod.Girls.Lillian });
    
        // Abia
        AddAnimation(1f, (seq, c) =>
        {
            seq.Append(c.Transform.DOShakeAnchorPos(4f, 30));
        }, new HashSet<RelativeId> { Hp2BaseMod.Girls.Abia });
    }
}
