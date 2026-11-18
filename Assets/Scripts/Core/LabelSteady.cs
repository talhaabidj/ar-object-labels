using UnityEngine;

namespace ArObjectLabels.Core
{
    /// <summary>What a label remembers from the frame before, so that it can stay steady.</summary>
    public class LabelMemory
    {
        /// <summary>Whether the label was among those shown.</summary>
        public bool WasShown;

        /// <summary>The move away from its own place that the label has settled on, in the flat picture.</summary>
        public Vector2 HeldMove;

        /// <summary>For how many seconds the label has had no reason left to be away from its own place.</summary>
        public float CalmSeconds;

        /// <summary>The move it is drawn with now, on its way to the held one.</summary>
        public Vector2 DrawnMove;

        /// <summary>False until the label has been drawn once; a new label starts at its place without gliding there.</summary>
        public bool HasBeenDrawn;

        public void Forget()
        {
            WasShown = false;
            HeldMove = Vector2.zero;
            CalmSeconds = 0f;
            DrawnMove = Vector2.zero;
            HasBeenDrawn = false;
        }
    }

    /// <summary>Keeps labels from blinking and hopping from frame to frame.</summary>
    public static class LabelSteady
    {
        // Moves closer than this, in the flat picture one metre away, count as the same move.
        const float SameMove = 0.0005f;

        /// <summary>
        /// The size a label's object counts with when the labels to show are chosen. A label that is shown
        /// already counts as bigger by the bonus, so another object must clearly beat it to take its place.
        /// </summary>
        public static float SizeForChoice(float size, bool wasShown, float stayBonus)
        {
            return wasShown ? size * (1f + stayBonus) : size;
        }

        /// <summary>
        /// The move a label settles on in this frame. <paramref name="needed"/> is what the rule for free
        /// places asks for now. A label at its own place moves as soon as there is a reason. A moved label
        /// keeps its place while that place is free; when no move is needed any more it still waits
        /// <paramref name="holdSeconds"/> before it goes back, so a reason that comes and goes does not make
        /// it hop.
        /// </summary>
        public static Vector2 Move(LabelMemory memory, bool heldPlaceIsFree, Vector2 needed, float secondsSinceLast, float holdSeconds)
        {
            Vector2 held = memory.HeldMove;
            bool heldIsOwnPlace = held.sqrMagnitude < SameMove * SameMove;
            bool noMoveNeeded = needed.sqrMagnitude < SameMove * SameMove;
            if (heldIsOwnPlace || !heldPlaceIsFree)
            {
                memory.CalmSeconds = 0f;
                return needed;
            }
            if (!noMoveNeeded)
            {
                memory.CalmSeconds = 0f;
                return held;
            }
            memory.CalmSeconds += secondsSinceLast;
            return memory.CalmSeconds >= holdSeconds ? Vector2.zero : held;
        }

        /// <summary>
        /// One frame's step of a drawn move towards the move it should have. It covers most of the way in
        /// <paramref name="glideSeconds"/> and arrives exactly; with no glide time it is there at once.
        /// </summary>
        public static Vector2 Glide(Vector2 drawn, Vector2 target, float secondsSinceLast, float glideSeconds)
        {
            if (glideSeconds <= 0f || (target - drawn).sqrMagnitude < SameMove * SameMove)
            {
                return target;
            }
            // After the glide time, one twentieth of the way is left.
            float share = 1f - Mathf.Exp(-3f * secondsSinceLast / glideSeconds);
            return Vector2.Lerp(drawn, target, share);
        }
    }
}
