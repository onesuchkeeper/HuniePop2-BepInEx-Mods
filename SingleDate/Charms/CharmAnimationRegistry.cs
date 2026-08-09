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
        _animations.Add(new CharmAnimationDefinition
        {
            Weight = 1.4f,
            Cost = 1.0f,
            AllowedCharacters = null,
            Build = (seq, c) =>
            {
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

        // Sarah
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

        // Lillian
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

        // Abia
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

        // Ashley
        // Brooke
        // Candace
        // Jessie
        // Jewn
        // Kyu
        // Lailani
        // Lola
        // Moxie
        // Nora
        // Polly
        // Zoey
    }
}