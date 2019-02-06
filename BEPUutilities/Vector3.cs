﻿
using System;

namespace BEPUutilities
{
    /// <summary>
    /// Provides XNA-like 3D vector math.
    /// </summary>
    public struct Vector3 : IEquatable<Vector3>
    {
        /// <summary>
        /// X component of the vector.
        /// </summary>
        public Fix32 X;
        /// <summary>
        /// Y component of the vector.
        /// </summary>
        public Fix32 Y;
        /// <summary>
        /// Z component of the vector.
        /// </summary>
        public Fix32 Z;

        /// <summary>
        /// Constructs a new 3d vector.
        /// </summary>
        /// <param name="x">X component of the vector.</param>
        /// <param name="y">Y component of the vector.</param>
        /// <param name="z">Z component of the vector.</param>
        public Vector3(Fix32 x, Fix32 y, Fix32 z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        /// <summary>
        /// Constructs a new 3d vector.
        /// </summary>
        /// <param name="xy">X and Y components of the vector.</param>
        /// <param name="z">Z component of the vector.</param>
        public Vector3(Vector2 xy, Fix32 z)
        {
            this.X = xy.X;
            this.Y = xy.Y;
            this.Z = z;
        }

        /// <summary>
        /// Constructs a new 3d vector.
        /// </summary>
        /// <param name="x">X component of the vector.</param>
        /// <param name="yz">Y and Z components of the vector.</param>
        public Vector3(Fix32 x, Vector2 yz)
        {
            this.X = x;
            this.Y = yz.X;
            this.Z = yz.Y;
        }

        /// <summary>
        /// Computes the squared length of the vector.
        /// </summary>
        /// <returns>Squared length of the vector.</returns>
        public Fix32 LengthSquared()
        {
            return ((X.Mul(X)).Add(Y.Mul(Y))).Add(Z.Mul(Z));
        }

        /// <summary>
        /// Computes the length of the vector.
        /// </summary>
        /// <returns>Length of the vector.</returns>
        public Fix32 Length()
        {
            return Fix32Ext.Sqrt(((X.Mul(X)).Add(Y.Mul(Y))).Add(Z.Mul(Z)));
        }

        /// <summary>
        /// Normalizes the vector.
        /// </summary>
        public void Normalize()
        {
            Fix32 inverse = F64.C1.Div(Fix32Ext.Sqrt(((X.Mul(X)).Add(Y.Mul(Y))).Add(Z.Mul(Z))));
			X = X.Mul(inverse);
			Y = Y.Mul(inverse);
			Z = Z.Mul(inverse);
        }

        /// <summary>
        /// Gets a string representation of the vector.
        /// </summary>
        /// <returns>String representing the vector.</returns>
        public override string ToString()
        {
            return "{" + X + ", " + Y + ", " + Z + "}";
        }

        /// <summary>
        /// Computes the dot product of two vectors.
        /// </summary>
        /// <param name="a">First vector in the product.</param>
        /// <param name="b">Second vector in the product.</param>
        /// <returns>Resulting dot product.</returns>
        public static Fix32 Dot(Vector3 a, Vector3 b)
        {
            return ((a.X.Mul(b.X)).Add(a.Y.Mul(b.Y))).Add(a.Z.Mul(b.Z));
        }

        /// <summary>
        /// Computes the dot product of two vectors.
        /// </summary>
        /// <param name="a">First vector in the product.</param>
        /// <param name="b">Second vector in the product.</param>
        /// <param name="product">Resulting dot product.</param>
        public static void Dot(ref Vector3 a, ref Vector3 b, out Fix32 product)
        {
            product = ((a.X.Mul(b.X)).Add(a.Y.Mul(b.Y))).Add(a.Z.Mul(b.Z));
        }
        /// <summary>
        /// Adds two vectors together.
        /// </summary>
        /// <param name="a">First vector to add.</param>
        /// <param name="b">Second vector to add.</param>
        /// <param name="sum">Sum of the two vectors.</param>
        public static void Add(ref Vector3 a, ref Vector3 b, out Vector3 sum)
        {
            sum.X = a.X.Add(b.X);
            sum.Y = a.Y.Add(b.Y);
            sum.Z = a.Z.Add(b.Z);
        }
        /// <summary>
        /// Subtracts two vectors.
        /// </summary>
        /// <param name="a">Vector to subtract from.</param>
        /// <param name="b">Vector to subtract from the first vector.</param>
        /// <param name="difference">Result of the subtraction.</param>
        public static void Subtract(ref Vector3 a, ref Vector3 b, out Vector3 difference)
        {
            difference.X = a.X.Sub(b.X);
            difference.Y = a.Y.Sub(b.Y);
            difference.Z = a.Z.Sub(b.Z);
        }
        /// <summary>
        /// Scales a vector.
        /// </summary>
        /// <param name="v">Vector to scale.</param>
        /// <param name="scale">Amount to scale.</param>
        /// <param name="result">Scaled vector.</param>
        public static void Multiply(ref Vector3 v, Fix32 scale, out Vector3 result)
        {
            result.X = v.X.Mul(scale);
            result.Y = v.Y.Mul(scale);
            result.Z = v.Z.Mul(scale);
        }

        /// <summary>
        /// Multiplies two vectors on a per-component basis.
        /// </summary>
        /// <param name="a">First vector to multiply.</param>
        /// <param name="b">Second vector to multiply.</param>
        /// <param name="result">Result of the componentwise multiplication.</param>
        public static void Multiply(ref Vector3 a, ref Vector3 b, out Vector3 result)
        {
            result.X = a.X.Mul(b.X);
            result.Y = a.Y.Mul(b.Y);
            result.Z = a.Z.Mul(b.Z);
        }

        /// <summary>
        /// Divides a vector's components by some amount.
        /// </summary>
        /// <param name="v">Vector to divide.</param>
        /// <param name="divisor">Value to divide the vector's components.</param>
        /// <param name="result">Result of the division.</param>
        public static void Divide(ref Vector3 v, Fix32 divisor, out Vector3 result)
        {
            Fix32 inverse = F64.C1.Div(divisor);
            result.X = v.X.Mul(inverse);
            result.Y = v.Y.Mul(inverse);
            result.Z = v.Z.Mul(inverse);
        }
        /// <summary>
        /// Scales a vector.
        /// </summary>
        /// <param name="v">Vector to scale.</param>
        /// <param name="f">Amount to scale.</param>
        /// <returns>Scaled vector.</returns>
        public static Vector3 operator *(Vector3 v, Fix32 f)
        {
            Vector3 toReturn;
            toReturn.X = v.X.Mul(f);
            toReturn.Y = v.Y.Mul(f);
            toReturn.Z = v.Z.Mul(f);
            return toReturn;
        }

        /// <summary>
        /// Scales a vector.
        /// </summary>
        /// <param name="v">Vector to scale.</param>
        /// <param name="f">Amount to scale.</param>
        /// <returns>Scaled vector.</returns>
        public static Vector3 operator *(Fix32 f, Vector3 v)
        {
            Vector3 toReturn;
            toReturn.X = v.X.Mul(f);
            toReturn.Y = v.Y.Mul(f);
            toReturn.Z = v.Z.Mul(f);
            return toReturn;
        }

        /// <summary>
        /// Multiplies two vectors on a per-component basis.
        /// </summary>
        /// <param name="a">First vector to multiply.</param>
        /// <param name="b">Second vector to multiply.</param>
        /// <returns>Result of the componentwise multiplication.</returns>
        public static Vector3 operator *(Vector3 a, Vector3 b)
        {
            Vector3 result;
            Multiply(ref a, ref b, out result);
            return result;
        }

        /// <summary>
        /// Divides a vector's components by some amount.
        /// </summary>
        /// <param name="v">Vector to divide.</param>
        /// <param name="f">Value to divide the vector's components.</param>
        /// <returns>Result of the division.</returns>
        public static Vector3 operator /(Vector3 v, Fix32 f)
        {
            Vector3 toReturn;
            f = F64.C1.Div(f);
            toReturn.X = v.X.Mul(f);
            toReturn.Y = v.Y.Mul(f);
            toReturn.Z = v.Z.Mul(f);
            return toReturn;
        }
        /// <summary>
        /// Subtracts two vectors.
        /// </summary>
        /// <param name="a">Vector to subtract from.</param>
        /// <param name="b">Vector to subtract from the first vector.</param>
        /// <returns>Result of the subtraction.</returns>
        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            Vector3 v;
            v.X = a.X.Sub(b.X);
            v.Y = a.Y.Sub(b.Y);
            v.Z = a.Z.Sub(b.Z);
            return v;
        }
        /// <summary>
        /// Adds two vectors together.
        /// </summary>
        /// <param name="a">First vector to add.</param>
        /// <param name="b">Second vector to add.</param>
        /// <returns>Sum of the two vectors.</returns>
        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            Vector3 v;
            v.X = a.X.Add(b.X);
            v.Y = a.Y.Add(b.Y);
            v.Z = a.Z.Add(b.Z);
            return v;
        }


        /// <summary>
        /// Negates the vector.
        /// </summary>
        /// <param name="v">Vector to negate.</param>
        /// <returns>Negated vector.</returns>
        public static Vector3 operator -(Vector3 v)
        {
            v.X = v.X.Neg();
            v.Y = v.Y.Neg();
            v.Z = v.Z.Neg();
            return v;
        }
        /// <summary>
        /// Tests two vectors for componentwise equivalence.
        /// </summary>
        /// <param name="a">First vector to test for equivalence.</param>
        /// <param name="b">Second vector to test for equivalence.</param>
        /// <returns>Whether the vectors were equivalent.</returns>
        public static bool operator ==(Vector3 a, Vector3 b)
        {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        }
        /// <summary>
        /// Tests two vectors for componentwise inequivalence.
        /// </summary>
        /// <param name="a">First vector to test for inequivalence.</param>
        /// <param name="b">Second vector to test for inequivalence.</param>
        /// <returns>Whether the vectors were inequivalent.</returns>
        public static bool operator !=(Vector3 a, Vector3 b)
        {
            return a.X != b.X || a.Y != b.Y || a.Z != b.Z;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(Vector3 other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <returns>
        /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        /// <param name="obj">Another object to compare to. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (obj is Vector3)
            {
                return Equals((Vector3)obj);
            }
            return false;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            return X.GetHashCode() + Y.GetHashCode() + Z.GetHashCode();
        }

        
        /// <summary>
        /// Computes the squared distance between two vectors.
        /// </summary>
        /// <param name="a">First vector.</param>
        /// <param name="b">Second vector.</param>
        /// <param name="distanceSquared">Squared distance between the two vectors.</param>
        public static void DistanceSquared(ref Vector3 a, ref Vector3 b, out Fix32 distanceSquared)
        {
            Fix32 x = a.X.Sub(b.X);
            Fix32 y = a.Y.Sub(b.Y);
            Fix32 z = a.Z.Sub(b.Z);
            distanceSquared = ((x.Mul(x)).Add(y.Mul(y))).Add(z.Mul(z));
        }

        /// <summary>
        /// Computes the squared distance between two vectors.
        /// </summary>
        /// <param name="a">First vector.</param>
        /// <param name="b">Second vector.</param>
        /// <returns>Squared distance between the two vectors.</returns>
        public static Fix32 DistanceSquared(Vector3 a, Vector3 b)
        {
            Fix32 x = a.X.Sub(b.X);
            Fix32 y = a.Y.Sub(b.Y);
            Fix32 z = a.Z.Sub(b.Z);
            return ((x.Mul(x)).Add(y.Mul(y))).Add(z.Mul(z));
        }


        /// <summary>
        /// Computes the distance between two two vectors.
        /// </summary>
        /// <param name="a">First vector.</param>
        /// <param name="b">Second vector.</param>
        /// <param name="distance">Distance between the two vectors.</param>
        public static void Distance(ref Vector3 a, ref Vector3 b, out Fix32 distance)
        {
            Fix32 x = a.X.Sub(b.X);
            Fix32 y = a.Y.Sub(b.Y);
            Fix32 z = a.Z.Sub(b.Z);
            distance = Fix32Ext.Sqrt(((x.Mul(x)).Add(y.Mul(y))).Add(z.Mul(z)));
        }
        /// <summary>
        /// Computes the distance between two two vectors.
        /// </summary>
        /// <param name="a">First vector.</param>
        /// <param name="b">Second vector.</param>
        /// <returns>Distance between the two vectors.</returns>
        public static Fix32 Distance(Vector3 a, Vector3 b)
        {
            Fix32 toReturn;
            Distance(ref a, ref b, out toReturn);
            return toReturn;
        }

        /// <summary>
        /// Gets the zero vector.
        /// </summary>
        public static Vector3 Zero
        {
            get
            {
                return new Vector3();
            }
        }

        /// <summary>
        /// Gets the up vector (0,1,0).
        /// </summary>
        public static Vector3 Up
        {
            get
            {
                return new Vector3()
                {
                    X = F64.C0,
                    Y = F64.C1,
                    Z = F64.C0
				};
            }
        }

        /// <summary>
        /// Gets the down vector (0,-1,0).
        /// </summary>
        public static Vector3 Down
        {
            get
            {
                return new Vector3()
                {
                    X = F64.C0,
                    Y = F64.C1.Neg(),
                    Z = F64.C0
				};
            }
        }

        /// <summary>
        /// Gets the right vector (1,0,0).
        /// </summary>
        public static Vector3 Right
        {
            get
            {
                return new Vector3()
                {
                    X = F64.C1,
                    Y = F64.C0,
                    Z = F64.C0
				};
            }
        }

        /// <summary>
        /// Gets the left vector (-1,0,0).
        /// </summary>
        public static Vector3 Left
        {
            get
            {
                return new Vector3()
                {
                    X = F64.C1.Neg(),
                    Y = F64.C0,
                    Z = F64.C0
				};
            }
        }

        /// <summary>
        /// Gets the forward vector (0,0,-1).
        /// </summary>
        public static Vector3 Forward
        {
            get
            {
                return new Vector3()
                {
                    X = F64.C0,
                    Y = F64.C0,
                    Z = F64.C1.Neg()
				};
            }
        }

        /// <summary>
        /// Gets the back vector (0,0,1).
        /// </summary>
        public static Vector3 Backward
        {
            get
            {
                return new Vector3()
                {
                    X = F64.C0,
                    Y = F64.C0,
                    Z = F64.C1
				};
            }
        }

        /// <summary>
        /// Gets a vector pointing along the X axis.
        /// </summary>
        public static Vector3 UnitX
        {
            get { return new Vector3 { X = F64.C1 }; }
        }

        /// <summary>
        /// Gets a vector pointing along the Y axis.
        /// </summary>
        public static Vector3 UnitY
        {
            get { return new Vector3 { Y = F64.C1 }; }
        }

        /// <summary>
        /// Gets a vector pointing along the Z axis.
        /// </summary>
        public static Vector3 UnitZ
        {
            get { return new Vector3 { Z = F64.C1 }; }
        }

        /// <summary>
        /// Computes the cross product between two vectors.
        /// </summary>
        /// <param name="a">First vector.</param>
        /// <param name="b">Second vector.</param>
        /// <returns>Cross product of the two vectors.</returns>
        public static Vector3 Cross(Vector3 a, Vector3 b)
        {
            Vector3 toReturn;
            Vector3.Cross(ref a, ref b, out toReturn);
            return toReturn;
        }
        /// <summary>
        /// Computes the cross product between two vectors.
        /// </summary>
        /// <param name="a">First vector.</param>
        /// <param name="b">Second vector.</param>
        /// <param name="result">Cross product of the two vectors.</param>
        public static void Cross(ref Vector3 a, ref Vector3 b, out Vector3 result)
        {
            Fix32 resultX = (a.Y.Mul(b.Z)).Sub(a.Z.Mul(b.Y));
            Fix32 resultY = (a.Z.Mul(b.X)).Sub(a.X.Mul(b.Z));
            Fix32 resultZ = (a.X.Mul(b.Y)).Sub(a.Y.Mul(b.X));
            result.X = resultX;
            result.Y = resultY;
            result.Z = resultZ;
        }

        /// <summary>
        /// Normalizes the given vector.
        /// </summary>
        /// <param name="v">Vector to normalize.</param>
        /// <returns>Normalized vector.</returns>
        public static Vector3 Normalize(Vector3 v)
        {
            Vector3 toReturn;
            Vector3.Normalize(ref v, out toReturn);
            return toReturn;
        }

        /// <summary>
        /// Normalizes the given vector.
        /// </summary>
        /// <param name="v">Vector to normalize.</param>
        /// <param name="result">Normalized vector.</param>
        public static void Normalize(ref Vector3 v, out Vector3 result)
        {
            Fix32 inverse = F64.C1.Div(Fix32Ext.Sqrt(((v.X.Mul(v.X)).Add(v.Y.Mul(v.Y))).Add(v.Z.Mul(v.Z))));
            result.X = v.X.Mul(inverse);
            result.Y = v.Y.Mul(inverse);
            result.Z = v.Z.Mul(inverse);
        }

        /// <summary>
        /// Negates a vector.
        /// </summary>
        /// <param name="v">Vector to negate.</param>
        /// <param name="negated">Negated vector.</param>
        public static void Negate(ref Vector3 v, out Vector3 negated)
        {
            negated.X = v.X.Neg();
            negated.Y = v.Y.Neg();
            negated.Z = v.Z.Neg();
        }

        /// <summary>
        /// Computes the absolute value of the input vector.
        /// </summary>
        /// <param name="v">Vector to take the absolute value of.</param>
        /// <param name="result">Vector with nonnegative elements.</param>
        public static void Abs(ref Vector3 v, out Vector3 result)
        {
            if (v.X < F64.C0)
                result.X = v.X.Neg();
            else
                result.X = v.X;
            if (v.Y < F64.C0)
                result.Y = v.Y.Neg();
            else
                result.Y = v.Y;
            if (v.Z < F64.C0)
                result.Z = v.Z.Neg();
            else
                result.Z = v.Z;
        }

        /// <summary>
        /// Computes the absolute value of the input vector.
        /// </summary>
        /// <param name="v">Vector to take the absolute value of.</param>
        /// <returns>Vector with nonnegative elements.</returns>
        public static Vector3 Abs(Vector3 v)
        {
            Vector3 result;
            Abs(ref v, out result);
            return result;
        }

        /// <summary>
        /// Creates a vector from the lesser values in each vector.
        /// </summary>
        /// <param name="a">First input vector to compare values from.</param>
        /// <param name="b">Second input vector to compare values from.</param>
        /// <param name="min">Vector containing the lesser values of each vector.</param>
        public static void Min(ref Vector3 a, ref Vector3 b, out Vector3 min)
        {
            min.X = a.X < b.X ? a.X : b.X;
            min.Y = a.Y < b.Y ? a.Y : b.Y;
            min.Z = a.Z < b.Z ? a.Z : b.Z;
        }

        /// <summary>
        /// Creates a vector from the lesser values in each vector.
        /// </summary>
        /// <param name="a">First input vector to compare values from.</param>
        /// <param name="b">Second input vector to compare values from.</param>
        /// <returns>Vector containing the lesser values of each vector.</returns>
        public static Vector3 Min(Vector3 a, Vector3 b)
        {
            Vector3 result;
            Min(ref a, ref b, out result);
            return result;
        }


        /// <summary>
        /// Creates a vector from the greater values in each vector.
        /// </summary>
        /// <param name="a">First input vector to compare values from.</param>
        /// <param name="b">Second input vector to compare values from.</param>
        /// <param name="max">Vector containing the greater values of each vector.</param>
        public static void Max(ref Vector3 a, ref Vector3 b, out Vector3 max)
        {
            max.X = a.X > b.X ? a.X : b.X;
            max.Y = a.Y > b.Y ? a.Y : b.Y;
            max.Z = a.Z > b.Z ? a.Z : b.Z;
        }

        /// <summary>
        /// Creates a vector from the greater values in each vector.
        /// </summary>
        /// <param name="a">First input vector to compare values from.</param>
        /// <param name="b">Second input vector to compare values from.</param>
        /// <returns>Vector containing the greater values of each vector.</returns>
        public static Vector3 Max(Vector3 a, Vector3 b)
        {
            Vector3 result;
            Max(ref a, ref b, out result);
            return result;
        }

        /// <summary>
        /// Computes an interpolated state between two vectors.
        /// </summary>
        /// <param name="start">Starting location of the interpolation.</param>
        /// <param name="end">Ending location of the interpolation.</param>
        /// <param name="interpolationAmount">Amount of the end location to use.</param>
        /// <returns>Interpolated intermediate state.</returns>
        public static Vector3 Lerp(Vector3 start, Vector3 end, Fix32 interpolationAmount)
        {
            Vector3 toReturn;
            Lerp(ref start, ref end, interpolationAmount, out toReturn);
            return toReturn;
        }
        /// <summary>
        /// Computes an interpolated state between two vectors.
        /// </summary>
        /// <param name="start">Starting location of the interpolation.</param>
        /// <param name="end">Ending location of the interpolation.</param>
        /// <param name="interpolationAmount">Amount of the end location to use.</param>
        /// <param name="result">Interpolated intermediate state.</param>
        public static void Lerp(ref Vector3 start, ref Vector3 end, Fix32 interpolationAmount, out Vector3 result)
        {
            Fix32 startAmount = F64.C1.Sub(interpolationAmount);
            result.X = (start.X.Mul(startAmount)).Add(end.X.Mul(interpolationAmount));
            result.Y = (start.Y.Mul(startAmount)).Add(end.Y.Mul(interpolationAmount));
            result.Z = (start.Z.Mul(startAmount)).Add(end.Z.Mul(interpolationAmount));
        }

        /// <summary>
        /// Computes an intermediate location using hermite interpolation.
        /// </summary>
        /// <param name="value1">First position.</param>
        /// <param name="tangent1">Tangent associated with the first position.</param>
        /// <param name="value2">Second position.</param>
        /// <param name="tangent2">Tangent associated with the second position.</param>
        /// <param name="interpolationAmount">Amount of the second point to use.</param>
        /// <param name="result">Interpolated intermediate state.</param>
        public static void Hermite(ref Vector3 value1, ref Vector3 tangent1, ref Vector3 value2, ref Vector3 tangent2, Fix32 interpolationAmount, out Vector3 result)
        {
            Fix32 weightSquared = interpolationAmount.Mul(interpolationAmount);
            Fix32 weightCubed = interpolationAmount.Mul(weightSquared);
            Fix32 value1Blend = ((F64.C2.Mul(weightCubed)).Sub(F64.C3.Mul(weightSquared))).Add(F64.C1);
            Fix32 tangent1Blend = (weightCubed.Sub(F64.C2.Mul(weightSquared))).Add(interpolationAmount);
            Fix32 value2Blend = ((F64.C2.Neg()).Mul(weightCubed)).Add(F64.C3.Mul(weightSquared));
            Fix32 tangent2Blend = weightCubed.Sub(weightSquared);
            result.X = (((value1.X.Mul(value1Blend)).Add(value2.X.Mul(value2Blend))).Add(tangent1.X.Mul(tangent1Blend))).Add(tangent2.X.Mul(tangent2Blend));
            result.Y = (((value1.Y.Mul(value1Blend)).Add(value2.Y.Mul(value2Blend))).Add(tangent1.Y.Mul(tangent1Blend))).Add(tangent2.Y.Mul(tangent2Blend));
            result.Z = (((value1.Z.Mul(value1Blend)).Add(value2.Z.Mul(value2Blend))).Add(tangent1.Z.Mul(tangent1Blend))).Add(tangent2.Z.Mul(tangent2Blend));
        }
        /// <summary>
        /// Computes an intermediate location using hermite interpolation.
        /// </summary>
        /// <param name="value1">First position.</param>
        /// <param name="tangent1">Tangent associated with the first position.</param>
        /// <param name="value2">Second position.</param>
        /// <param name="tangent2">Tangent associated with the second position.</param>
        /// <param name="interpolationAmount">Amount of the second point to use.</param>
        /// <returns>Interpolated intermediate state.</returns>
        public static Vector3 Hermite(Vector3 value1, Vector3 tangent1, Vector3 value2, Vector3 tangent2, Fix32 interpolationAmount)
        {
            Vector3 toReturn;
            Hermite(ref value1, ref tangent1, ref value2, ref tangent2, interpolationAmount, out toReturn);
            return toReturn;
        }
    }
}
