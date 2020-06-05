using UnityEngine;

public class CodeUtils
{
    #region bitfield
    // Toggle the bit n between on/off in field
    public static void BitfieldToggle(int bit, ref uint field)
    {
        field ^= (1u << bit);
    }


    // Set the bit n to true/false in field
    public static void BitfieldSet(int bit, bool state, ref uint field)
    {
        if (state)
            field |= (1u << bit);
        else
            field &= ~(1u << bit);
    }


    // Set the first nth bit to true
    public static void BitfieldSetAllTrue(int nb, ref uint field)
    {
        field = (1u << nb) - 1;
    }


    // Return true if bit is set to true
    public static bool BitfieldCheck(int bit, ref uint field)
    {
        int mask = 1 << bit;
        return (mask & field) == mask;
    }
    #endregion


    //-------------------------------------------------------------------------
    // Sort of bounding sphere to compute centre of n points
    //-------------------------------------------------------------------------
    public class BSphere
    {
        public Vector3 Pos;
        public float radius;

        public BSphere()
        {
            Pos = Vector3.zero;
            radius = -1.0f; // indicate an invalid sphere (not initialized)
        }

        public BSphere(Vector3 _pos, float _rad)
        {
            Pos = _pos;
            radius = _rad;
        }

        // Based on miniBall, compute enclosing sphere approximation
        static public BSphere SmallBall(Vector3[] P, int p)
        {
            Vector3 centre = Vector3.zero;
            float radius = -1.0f;

            if (p > 0)
            {
                foreach (Vector3 c0 in P)
                {
                    centre += c0;
                }
                centre = centre / p;

                foreach (Vector3 c0 in P)
                {
                    float d2 = (c0 - centre).sqrMagnitude;

                    if (d2 > radius)
                    {
                        radius = d2;
                    }
                }

                radius = Mathf.Sqrt(radius) + float.Epsilon; // to avoid numerical inaccuracies
            }

            BSphere s = new BSphere(centre, radius);
            return s;
        }
    }
}