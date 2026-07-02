using System;
using System.Collections.Generic;
using DG.Tweening;
using Hp2BaseMod;
using UnityEngine;

namespace HuniePopUltimate;

public static class SingleDateCharmAnimation
{
    public static void AddCharmAnimations(Action<float, float, Func<Sequence, (RectTransform transform, bool dir, RelativeId girlId), bool>, HashSet<RelativeId>> m_RegisterCharmAnimation)
    {
        // =========================
        // AIKO
        // =========================
        m_RegisterCharmAnimation(
            0.9f,
            0.8f,
            (Sequence seq, (RectTransform Transform, bool Dir, RelativeId GirlId) context) =>
            {
                // Lazy collapse: body sinks into disinterest
                seq.Append(context.Transform.DOLocalMoveY(context.Transform.localPosition.y - 15f, 1.5f).SetEase(Ease.InOutSine));
                seq.Join(context.Transform.DOScale(1.02f, 1.5f));

                // long idle hold (detached / unmotivated pause)
                seq.AppendInterval(2.5f);

                // slow recovery back to baseline awareness
                seq.Append(context.Transform.DOLocalMoveY(context.Transform.localPosition.y, 1.8f).SetEase(Ease.InOutSine));
                seq.Join(context.Transform.DOScale(1f, 1.8f));

                return false;
            },
            new HashSet<RelativeId> { Girls.Aiko }
        );

        // EXTRA: Aiko - “barely reacts” micro acknowledgment
        m_RegisterCharmAnimation(
            0.75f,
            0.6f,
            (Sequence seq, (RectTransform Transform, bool Dir, RelativeId GirlId) context) =>
            {
                // minimal reaction: tiny shift only
                seq.Append(context.Transform.DOLocalMoveX(context.Transform.localPosition.x + (6f * (context.Dir ? 1 : -1)), 0.25f));

                // immediate disengagement
                seq.Append(context.Transform.DOLocalMoveX(context.Transform.localPosition.x, 0.35f));

                // faint jitter (almost accidental attention)
                seq.Append(context.Transform.DOShakeAnchorPos(0.3f, 2f, 6));

                return false;
            },
            new HashSet<RelativeId> { Girls.Aiko }
        );

        // =========================
        // AUDREY
        // =========================
        m_RegisterCharmAnimation(
            1.2f,
            1.4f,
            (Sequence seq, (RectTransform Transform, bool Dir, RelativeId GirlId) context) =>
            {
                // aggressive forward jab (dominance assertion)
                seq.Append(context.Transform.DOLocalMoveX(
                    context.Transform.localPosition.x + (30f * (context.Dir ? 1 : -1)), 0.12f));

                // unstable emotional sway (volatile personality spike)
                seq.Append(context.Transform.DOShakeAnchorPos(1.1f, 16f, 12));

                // recoil back with dismissive energy
                seq.Append(context.Transform.DOLocalMoveX(
                    context.Transform.localPosition.x - (10f * (context.Dir ? 1 : -1)), 0.4f));

                // stabilize into upright control state
                seq.Append(context.Transform.DOScale(1f, 0.6f));

                return false;
            },
            new HashSet<RelativeId> { Girls.Audrey }
        );

        // EXTRA: Audrey - “drunk sway instability loop”
        m_RegisterCharmAnimation(
            1.0f,
            1.1f,
            (Sequence seq, (RectTransform Transform, bool Dir, RelativeId GirlId) context) =>
            {
                // unstable lateral drift
                seq.Append(context.Transform.DOLocalMoveX(context.Transform.localPosition.x + (12f * (context.Dir ? 1 : -1)), 0.4f));

                // overcorrection snap back
                seq.Append(context.Transform.DOLocalMoveX(context.Transform.localPosition.x - (8f * (context.Dir ? 1 : -1)), 0.3f));

                // chaotic micro shake
                seq.Append(context.Transform.DOShakeAnchorPos(0.8f, 14f, 10));

                return false;
            },
            new HashSet<RelativeId> { Girls.Audrey }
        );

        // =========================
        // BELI
        // =========================
        m_RegisterCharmAnimation(
            0.8f,
            0.9f,
            (Sequence seq, (RectTransform Transform, bool Dir, RelativeId GirlId) context) =>
            {
                // inhale expansion (calm breath-like motion)
                seq.Append(context.Transform.DOScale(1.05f, 2.0f).SetEase(Ease.InOutSine));

                seq.Join(context.Transform.DOLocalMoveY(
                    context.Transform.localPosition.y + 10f, 2.0f));

                // meditative hold (stillness state)
                seq.AppendInterval(1.5f);

                // exhale return (grounding reset)
                seq.Append(context.Transform.DOScale(1f, 2.0f).SetEase(Ease.InOutSine));

                seq.Join(context.Transform.DOLocalMoveY(
                    context.Transform.localPosition.y, 2.0f));

                return false;
            },
            new HashSet<RelativeId> { Girls.Beli }
        );

        // EXTRA: Beli - “yoga stretch flow state”
        m_RegisterCharmAnimation(
            0.7f,
            0.8f,
            (Sequence seq, (RectTransform Transform, bool Dir, RelativeId GirlId) context) =>
            {
                // gentle upward stretch
                seq.Append(context.Transform.DOLocalMoveY(context.Transform.localPosition.y + 14f, 1.8f));

                // soft hold (balanced pose)
                seq.AppendInterval(1.2f);

                // slow return to center
                seq.Append(context.Transform.DOLocalMoveY(context.Transform.localPosition.y, 1.8f));

                return false;
            },
            new HashSet<RelativeId> { Girls.Beli }
        );

        // =========================
        // CELESTE
        // =========================
        m_RegisterCharmAnimation(
            0.75f,
            1.1f,
            (Sequence seq, (RectTransform Transform, bool Dir, RelativeId GirlId) context) =>
            {
                // precise analytical lean (measurement behavior)
                seq.Append(context.Transform.DOLocalMoveX(
                    context.Transform.localPosition.x + (16f * (context.Dir ? 1 : -1)),
                    0.5f).SetEase(Ease.Linear));

                // hard stop (no emotional smoothing)
                seq.AppendInterval(0.9f);

                // exact return to baseline
                seq.Append(context.Transform.DOLocalMoveX(
                    context.Transform.localPosition.x, 0.5f).SetEase(Ease.Linear));

                // micro diagnostic jitter (analysis scan)
                seq.Append(context.Transform.DOShakeAnchorPos(0.35f, 2f, 6));

                return false;
            },
            new HashSet<RelativeId> { Girls.Celeste }
        );

        // EXTRA: Celeste - “scan-and-evaluate motion”
        m_RegisterCharmAnimation(
            0.7f,
            1.0f,
            (Sequence seq, (RectTransform Transform, bool Dir, RelativeId GirlId) context) =>
            {
                // small vertical scan movement
                seq.Append(context.Transform.DOLocalMoveY(context.Transform.localPosition.y + 8f, 0.4f));

                // pause evaluation
                seq.AppendInterval(0.8f);

                // return to exact neutral
                seq.Append(context.Transform.DOLocalMoveY(context.Transform.localPosition.y, 0.4f));

                return false;
            },
            new HashSet<RelativeId> { Girls.Celeste }
        );

        // =========================
        // MOMO
        // =========================
        m_RegisterCharmAnimation(
            1.2f,
            0.7f,
            (Sequence seq, (RectTransform Transform, bool Dir, RelativeId GirlId) context) =>
            {
                // curious hop (animal-like excitement)
                seq.Append(context.Transform.DOScale(1.15f, 0.2f));
                seq.Join(context.Transform.DOLocalMoveY(context.Transform.localPosition.y + 10f, 0.2f));

                // playful instability wobble
                seq.Append(context.Transform.DOShakeAnchorPos(0.6f, 12f, 10));

                // soft landing normalization
                seq.Append(context.Transform.DOScale(1f, 0.5f));

                return false;
            },
            new HashSet<RelativeId> { Girls.Momo }
        );

        // EXTRA: Momo - “rapid playful darting”
        m_RegisterCharmAnimation(
            1.3f,
            0.6f,
            (Sequence seq, (RectTransform Transform, bool Dir, RelativeId GirlId) context) =>
            {
                // quick left-right dart
                seq.Append(context.Transform.DOLocalMoveX(context.Transform.localPosition.x + (10f * (context.Dir ? 1 : -1)), 0.15f));
                seq.Append(context.Transform.DOLocalMoveX(context.Transform.localPosition.x - (10f * (context.Dir ? 1 : -1)), 0.15f));

                // excited shake burst
                seq.Append(context.Transform.DOShakeAnchorPos(0.5f, 10f, 10));

                return false;
            },
            new HashSet<RelativeId> { Girls.Momo }
        );

        // =========================
        // KYANNA
        // =========================
        m_RegisterCharmAnimation(
            1.15f,
            0.9f,
            (Sequence seq, (RectTransform Transform, bool Dir, RelativeId GirlId) context) =>
            {
                // energetic bounce-in (fitness enthusiasm)
                seq.Append(context.Transform.DOScale(1.12f, 0.25f));
                seq.Join(context.Transform.DOLocalMoveY(context.Transform.localPosition.y + 8f, 0.25f));

                // excited emotional shake (high energy speech)
                seq.Append(context.Transform.DOShakeAnchorPos(0.6f, 10f, 12));

                // strong stable reset stance
                seq.Append(context.Transform.DOScale(1f, 0.4f));

                return false;
            },
            new HashSet<RelativeId> { Girls.Kyanna }
        );

        // EXTRA: Kyanna - “pump-up stance”
        m_RegisterCharmAnimation(
            1.2f,
            0.8f,
            (Sequence seq, (RectTransform Transform, bool Dir, RelativeId GirlId) context) =>
            {
                // assertive upward pump
                seq.Append(context.Transform.DOLocalMoveY(context.Transform.localPosition.y + 12f, 0.3f));

                // hold confidence pose
                seq.AppendInterval(0.8f);

                // return with controlled energy
                seq.Append(context.Transform.DOLocalMoveY(context.Transform.localPosition.y, 0.4f));

                return false;
            },
            new HashSet<RelativeId> { Girls.Kyanna }
        );

        // =========================
        // NIKKI
        // =========================
        m_RegisterCharmAnimation(
            0.85f,
            1.0f,
            (Sequence seq, (RectTransform Transform, bool Dir, RelativeId GirlId) context) =>
            {
                // shy retreat (introverted avoidance)
                seq.Append(context.Transform.DOLocalMoveX(
                    context.Transform.localPosition.x - (18f * (context.Dir ? 1 : -1)), 0.4f));

                // hesitation pause
                seq.AppendInterval(1.2f);

                // cautious return
                seq.Append(context.Transform.DOLocalMoveX(context.Transform.localPosition.x, 0.6f));
                seq.Join(context.Transform.DOScale(0.98f, 0.6f));

                // nervous micro shake
                seq.Append(context.Transform.DOShakeAnchorPos(0.6f, 6f, 12));

                return false;
            },
            new HashSet<RelativeId> { Girls.Nikki }
        );

        // EXTRA: Nikki - “panic correction flutter”
        m_RegisterCharmAnimation(
            0.8f,
            0.7f,
            (Sequence seq, (RectTransform Transform, bool Dir, RelativeId GirlId) context) =>
            {
                // small jitter burst (social anxiety spike)
                seq.Append(context.Transform.DOShakeAnchorPos(0.5f, 8f, 10));

                // immediate recoil inward
                seq.Append(context.Transform.DOLocalMoveX(context.Transform.localPosition.x, 0.3f));

                return false;
            },
            new HashSet<RelativeId> { Girls.Nikki }
        );

        // =========================
        // VENUS
        // =========================
        m_RegisterCharmAnimation(
            0.8f,
            1.3f,
            (Sequence seq, (RectTransform Transform, bool Dir, RelativeId GirlId) context) =>
            {
                // divine hover (controlled superiority)
                seq.Append(context.Transform.DOLocalMoveY(
                    context.Transform.localPosition.y + 12f, 0.6f));

                seq.Join(context.Transform.DOScale(1.06f, 0.6f));

                // judgmental pause
                seq.AppendInterval(1.2f);

                // elegant reset
                seq.Append(context.Transform.DOLocalMoveY(context.Transform.localPosition.y, 0.8f));
                seq.Join(context.Transform.DOScale(1f, 0.8f));

                return false;
            },
            new HashSet<RelativeId> { Girls.Venus }
        );

        // EXTRA: Venus - “inspection glide”
        m_RegisterCharmAnimation(
            0.75f,
            1.2f,
            (Sequence seq, (RectTransform Transform, bool Dir, RelativeId GirlId) context) =>
            {
                // slow lateral glide (evaluation pass)
                seq.Append(context.Transform.DOLocalMoveX(context.Transform.localPosition.x + (14f * (context.Dir ? 1 : -1)), 0.6f));

                // pause (judgment)
                seq.AppendInterval(1.0f);

                // return glide
                seq.Append(context.Transform.DOLocalMoveX(context.Transform.localPosition.x, 0.6f));

                return false;
            },
            new HashSet<RelativeId> { Girls.Venus }
        );

        // =========================
        // TIFFANY
        // =========================
        m_RegisterCharmAnimation(
            1.05f,
            0.9f,
            (Sequence seq, (RectTransform Transform, bool Dir, RelativeId GirlId) context) =>
            {
                // friendly step-in (warm engagement)
                seq.Append(context.Transform.DOLocalMoveX(
                    context.Transform.localPosition.x + (14f * (context.Dir ? 1 : -1)), 0.3f));

                seq.Join(context.Transform.DOScale(1.05f, 0.3f));

                // gentle hold
                seq.AppendInterval(0.9f);

                // soft return to neutral
                seq.Append(context.Transform.DOLocalMoveX(context.Transform.localPosition.x, 0.5f));

                seq.Join(context.Transform.DOScale(1f, 0.5f));

                return false;
            },
            new HashSet<RelativeId> { Girls.Tiffany }
        );

        // EXTRA: Tiffany - “helpful nod motion”
        m_RegisterCharmAnimation(
            1.0f,
            0.8f,
            (Sequence seq, (RectTransform Transform, bool Dir, RelativeId GirlId) context) =>
            {
                // slight vertical approval bounce
                seq.Append(context.Transform.DOLocalMoveY(context.Transform.localPosition.y + 6f, 0.25f));

                seq.Append(context.Transform.DOLocalMoveY(context.Transform.localPosition.y, 0.35f));

                return false;
            },
            new HashSet<RelativeId> { Girls.Tiffany }
        );
    }
}