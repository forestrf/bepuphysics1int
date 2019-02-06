﻿using FixMath.NET;
using System;

namespace BEPUutilities
{
    /// <summary>
    /// Provides XNA-like 4-component vector math.
    /// </summary>
    public struct Vector4 : IEquatable<Vector4>
    {
        /// <summary>
        /// X component of the vector.
        /// </summary>
        public Fix64 X;
        /// <summary>
        /// Y component of the vector.
        /// </summary>
        public Fix64 Y;
        /// <summary>
        /// Z component of the vector.
        /// </summary>
        public Fix64 Z;
        /// <summary>
        /// W component of the vector.
        /// </summary>
        public Fix64 W;

        /// <summary>
        /// Constructs a new 3d vector.
        /// </summary>
        /// <param name="x">X component of the vector.</param>
        /// <param name="y">Y component of the vector.</param>
        /// <param name="z">Z component of the vector.</param>
        /// <param name="w">W component of the vector.</param>
        public Vector4(Fix64 x, Fix64 y, Fix64 z, Fix64 w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }

        /// <summary>
        /// Constructs a new 3d vector.
        /// </summary>
        /// <param name="xyz">X, Y, and Z components of the vector.</param>
        /// <param name="w">W component of the vector.</param>
        public Vector4(Vector3 xyz, Fix64 w)
        {
            this.X = xyz.X;
            this.Y = xyz.Y;
            this.Z = xyz.Z;
            this.W = w;
        }


        /// <summary>
        /// Constructs a new 3d vector.
        /// </summary>
        /// <param name="x">X component of the vector.</param>
        /// <param name="yzw">Y, Z, and W components of the vector.</param>
        public Vector4(Fix64 x, Vector3 yzw)
        {
            this.X = x;
            this.Y = yzw.X;
            this.Z = yzw.Y;
            this.W = yzw.Z;
        }

        /// <summary>
        /// Constructs a new 3d vector.
        /// </summary>
        /// <param name="xy">X and Y components of the vector.</param>
        /// <param name="z">Z component of the vector.</param>
        /// <param name="w">W component of the vector.</param>
        public Vector4(Vector2 xy, Fix64 z, Fix64 w)
        {
            this.X = xy.X;
            this.Y = xy.Y;
            this.Z = z;
            this.W = w;
        }

        /// <summary>
        /// Constructs a new 3d vector.
        /// </summary>
        /// <param name="x">X component of the vector.</param>
        /// <param name="yz">Y and Z components of the vector.</param>
        /// <param name="w">W component of the vector.</param>
        public Vector4(Fix64 x, Vector2 yz, Fix64 w)
        {
            this.X = x;
            this.Y = yz.X;
            this.Z = yz.Y;
            this.W = w;
        }

        /// <summary>
        /// Constructs a new 3d vector.
        /// </summary>
        /// <param name="x">X component of the vector.</param>
        /// <param name="y">Y and Z components of the vector.</param>
        /// <param name="zw">W component of the vector.</param>
        public Vector4(Fix64 x, Fix64 y, Vector2 zw)
        {
            this.X = x;
            this.Y = y;
            this.Z = zw.X;
            this.W = zw.Y;
        }

        /// <summary>
        /// Constructs a new 3d vector.
        /// </summary>
        /// <param name="xy">X and Y components of the vector.</param>
        /// <param name="zw">Z and W components of the vector.</param>
        public Vector4(Vector2 xy, Vector2 zw)
        {
            this.X = xy.X;
            this.Y = xy.Y;
            this.Z = zw.X;
            this.W = zw.Y;
        }


        /// <summary>
        /// Computes the squared length of the vector.
        /// </summary>
        /// <returns>Squared length of the vector.</returns>
        public Fix64 LengthSquared()
        {
            return (((X.Mul(X)).Add(Y.Mul(Y))).Add(Z.Mul(Z))).Add(W.Mul(W));
        }

        /// <summary>
        /// Computes the length of the vector.
        /// </summary>
        /// <returns>Length of the vector.</returns>
        public Fix64 Length()
        {
            return Fix64.Sqrt((((X.Mul(X)).Add(Y.Mul(Y))).Add(Z.Mul(Z))).Add(W.Mul(W)));
        }

        /// <summary>
        /// Normalizes the vector.
        /// </summary>
        public void Normalize()
        {
            Fix64 inverse = F64.C1.Div(Fix64.Sqrt((((X.Mul(X)).Add(Y.Mul(Y))).Add(Z.Mul(Z))).Add(W.Mul(W))));
			X = X.Mul(inverse);
			Y = Y.Mul(inverse);
			Z = Z.Mul(inverse);
			W = W.Mul(inverse);
        }

        /// <summary>
        /// Gets a string representation of the vector.
        /// </summary>
        /// <returns>String representing the vector.</returns>
        public override string ToString()
        {
            return "{" + X + ", " + Y + ", " + Z + ", " + W + "}";
        }

        /// <summary>
        /// Computes the dot product of two vectors.
        /// </summary>
        /// <param name="a">First vector in the product.</param>
        /// <param name="b">Second vector in the product.</param>
        /// <returns>Resulting dot product.</returns>
        public static Fix64 Dot(Vector4 a, Vector4 b)
        {
            return (((a.X.Mul(b.X)).Add(a.Y.Mul(b.Y))).Add(a.Z.Mul(b.Z))).Add(a.W.Mul(b.W));
        }

        /// <summary>
        /// Computes the dot product of two vectors.
        /// </summary>
        /// <param name="a">First vector in the product.</param>
        /// <param name="b">Second vector in the product.</param>
        /// <param name="product">Resulting dot product.</param>
        public static void Dot(ref Vector4 a, ref Vector4 b, out Fix64 product)
        {
            product = (((a.X.Mul(b.X)).Add(a.Y.Mul(b.Y))).Add(a.Z.Mul(b.Z))).Add(a.W.Mul(b.W));
        }
        /// <summary>
        /// Adds two vectors together.
        /// </summary>
        /// <param name="a">First vector to add.</param>
        /// <param name="b">Second vector to add.</param>
        /// <param name="sum">Sum of the two vectors.</param>
        public static void Add(ref Vector4 a, ref Vector4 b, out Vector4 sum)
        {
            sum.X = a.X.Add(b.X);
            sum.Y = a.Y.Add(b.Y);
            sum.Z = a.Z.Add(b.Z);
            sum.W = a.W.Add(b.W);
        }
        /// <summary>
        /// Subtracts two vectors.
        /// </summary>
        /// <param name="a">Vector to subtract from.</param>
        /// <param name="b">Vector to subtract from the first vector.</param>
        /// <param name="difference">Result of the subtraction.</param>
        public static void Subtract(ref Vector4 a, ref Vector4 b, out Vector4 difference)
        {
            difference.X = a.X.Sub(b.X);
            difference.Y = a.Y.Sub(b.Y);
            difference.Z = a.Z.Sub(b.Z);
            difference.W = a.W.Sub(b.W);
        }
        /// <summary>
        /// Scales a vector.
        /// </summary>
        /// <param name="v">Vector to scale.</param>
        /// <param name="scale">Amount to scale.</param>
        /// <param name="result">Scaled vector.</param>
        public static void Multiply(ref Vector4 v, Fix64 scale, out Vector4 result)
        {
            result.X = v.X.Mul(scale);
            result.Y = v.Y.Mul(scale);
            result.Z = v.Z.Mul(scale);
            result.W = v.W.Mul(scale);
        }


        /// <summary>
        /// Multiplies two vectors on a per-component basis.
        /// </summary>
        /// <param name="a">First vector to multiply.</param>
        /// <param name="b">Second vector to multiply.</param>
        /// <param name="result">Result of the componentwise multiplication.</param>
        public static void Multiply(ref Vector4 a, ref Vector4 b, out Vector4 result)
        {
            result.X = a.X.Mul(b.X);
            result.Y = a.Y.Mul(b.Y);
            result.Z = a.Z.Mul(b.Z);
            result.W = a.W.Mul(b.W);
        }


        /// <summary>
        /// Divides a vector's components by some amount.
        /// </summary>
        /// <param name="v">Vector to divide.</param>
        /// <param name="divisor">Value to divide the vector's components.</param>
        /// <param name="result">Result of the division.</param>
        public static void Divide(ref Vector4 v, Fix64 divisor, out Vector4 result)
        {
            Fix64 inverse = F64.C1.Div(divisor);
            result.X = v.X.Mul(inverse);
            result.Y = v.Y.Mul(inverse);
            result.Z = v.Z.Mul(inverse);
            result.W = v.W.Mul(inverse);
        }
        /// <summary>
        /// Scales a vector.
        /// </summary>
        /// <param name="v">Vector to scale.</param>
        /// <param name="f">Amount to scale.</param>
        /// <returns>Scaled vector.</returns>
        public static Vector4 operator *(Vector4 v, Fix64 f)
        {
            Vector4 toReturn;
            toReturn.X = v.X.Mul(f);
            toReturn.Y = v.Y.Mul(f);
            toReturn.Z = v.Z.Mul(f);
            toReturn.W = v.W.Mul(f);
            return toReturn;
        }
        /// <summary>
        /// Scales a vector.
        /// </summary>
        /// <param name="v">Vector to scale.</param>
        /// <param name="f">Amount to scale.</param>
        /// <returns>Scaled vector.</returns>
        public static Vector4 operator *(Fix64 f, Vector4 v)
        {
            Vector4 toReturn;
            toReturn.X = v.X.Mul(f);
            toReturn.Y = v.Y.Mul(f);
            toReturn.Z = v.Z.Mul(f);
            toReturn.W = v.W.Mul(f);
            return toReturn;
        }


        /// <summary>
        /// Multiplies two vectors on a per-component basis.
        /// </summary>
        /// <param name="a">First vector to multiply.</param>
        /// <param name="b">Second vector to multiply.</param>
        /// <returns>Result of the componentwise multiplication.</returns>
        public static Vector4 operator *(Vector4 a, Vector4 b)
        {
            Vector4 result;
            Multiply(ref a, ref b, out result);
            return result;
        }


        /// <summary>
        /// Divides a vector's components by some amount.
        /// </summary>
        /// <param name="v">Vector to divide.</param>
        /// <param name="f">Value to divide the vector's components.</param>
        /// <returns>Result of the division.</returns>
        public static Vector4 operator /(Vector4 v, Fix64 f)
        {
            Vector4 toReturn;
            f = F64.C1.Div(f);
            toReturn.X = v.X.Mul(f);
            toReturn.Y = v.Y.Mul(f);
            toReturn.Z = v.Z.Mul(f);
            toReturn.W = v.W.Mul(f);
            return toReturn;
        }
        /// <summary>
        /// Subtracts two vectors.
        /// </summary>
        /// <param name="a">Vector to subtract from.</param>
        /// <param name="b">Vector to subtract from the first vector.</param>
        /// <returns>Result of the subtraction.</returns>
        public static Vector4 operator -(Vector4 a, Vector4 b)
        {
            Vector4 v;
            v.X = a.X.Sub(b.X);
            v.Y = a.Y.Sub(b.Y);
            v.Z = a.Z.Sub(b.Z);
            v.W = a.W.Sub(b.W);
            return v;
        }
        /// <summary>
        /// Adds two vectors together.
        /// </summary>
        /// <param name="a">First vector to add.</param>
        /// <param name="b">Second vector to add.</param>
        /// <returns>Sum of the two vectors.</returns>
        public static Vector4 operator +(Vector4 a, Vector4 b)
        {
            Vector4 v;
            v.X = a.X.Add(b.X);
            v.Y = a.Y.Add(b.Y);
            v.Z = a.Z.Add(b.Z);
            v.W = a.W.Add(b.W);
            return v;
        }


        /// <summary>
        /// Negates the vector.
        /// </summary>
        /// <param name="v">Vector to negate.</param>
        /// <returns>Negated vector.</returns>
        public static Vector4 operator -(Vector4 v)
        {
            v.X = v.X.Neg();
            v.Y = v.Y.Neg();
            v.Z = v.Z.Neg();
            v.W = v.W.Neg();
            return v;
        }
        /// <summary>
        /// Tests two vectors for componentwise equivalence.
        /// </summary>
        /// <param name="a">First vector to test for equivalence.</param>
        /// <param name="b">Second vector to test for equivalence.</param>
        /// <returns>Whether the vectors were equivalent.</returns>
        public static bool operator ==(Vector4 a, Vector4 b)
        {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z && a.W == b.W;
        }
        /// <summary>
        /// Tests two vectors for componentwise inequivalence.
        /// </summary>
        /// <param name="a">First vector to test for inequivalence.</param>
        /// <param name="b">Second vector to test for inequivalence.</param>
        /// <returns>Whether the vectors were inequivalent.</returns>
        public static bool operator !=(Vector4 a, Vector4 b)
        {
            return a.X != b.X || a.Y != b.Y || a.Z != b.Z || a.W != b.W;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(Vector4 other)
        {
            return X == other.X && Y == other.Y && Z == other.Z && W == other.W;
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
            if (obj is Vector4)
            {
                return Equals((Vector4)obj);
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
            return X.GetHashCode() + Y.GetHashCode() + Z.GetHashCode() + W.GetHashCode();
        }

        /// <summary>
        /// Computes the squared distance between two vectors.
        /// </summary>
        /// <param name="a">First vector.</param>
        /// <param name="b">Second vector.</param>
        /// <param name="distanceSquared">Squared distance between the two vectors.</param>
        public static void DistanceSquared(ref Vector4 a, ref Vector4 b, out Fix64 distanceSquared)
        {
            Fix64 x = a.X.Sub(b.X);
            Fix64 y = a.Y.Sub(b.Y);
            Fix64 z = a.Z.Sub(b.Z);
            Fix64 w = a.W.Sub(b.W);
            distanceSquared = (((x.Mul(x)).Add(y.Mul(y))).Add(z.Mul(z))).Add(w.Mul(w));
        }

        /// <summary>
        /// Computes the distance between two two vectors.
        /// </summary>
        /// <param name="a">First vector.</param>
        /// <param name="b">Second vector.</param>
        /// <param name="distance">Distance between the two vectors.</param>
        public static void Distance(ref Vector4 a, ref Vector4 b, out Fix64 distance)
        {
            Fix64 x = a.X.Sub(b.X);
            Fix64 y = a.Y.Sub(b.Y);
            Fix64 z = a.Z.Sub(b.Z);
            Fix64 w = a.W.Sub(b.W);
            distance = Fix64.Sqrt((((x.Mul(x)).Add(y.Mul(y))).Add(z.Mul(z))).Add(w.Mul(w)));
        }
        /// <summary>
        /// Computes the distance between two two vectors.
        /// </summary>
        /// <param name="a">First vector.</param>
        /// <param name="b">Second vector.</param>
        /// <returns>Distance between the two vectors.</returns>
        public static Fix64 Distance(Vector4 a, Vector4 b)
        {
            Fix64 toReturn;
            Distance(ref a, ref b, out toReturn);
            return toReturn;
        }

        /// <summary>
        /// Gets the zero vector.
        /// </summary>
        public static Vector4 Zero
        {
            get
            {
                return new Vector4();
            }
        }

        /// <summary>
        /// Gets a vector pointing along the X axis.
        /// </summary>
        public static Vector4 UnitX
        {
            get { return new Vector4 { X = F64.C1 }; }
        }

        /// <summary>
        /// Gets a vector pointing along the Y axis.
        /// </summary>
        public static Vector4 UnitY
        {
            get { return new Vector4 { Y = F64.C1 }; }
        }

        /// <summary>
        /// Gets a vector pointing along the Z axis.
        /// </summary>
        public static Vector4 UnitZ
        {
            get { return new Vector4 { Z = F64.C1 }; }
        }

        /// <summary>
        /// Gets a vector pointing along the W axis.
        /// </summary>
        public static Vector4 UnitW
        {
            get { return new Vector4 { W = F64.C1 }; }
        }

        /// <summary>
        /// Normalizes the given vector.
        /// </summary>
        /// <param name="v">Vector to normalize.</param>
        /// <returns>Normalized vector.</returns>
        public static Vector4 Normalize(Vector4 v)
        {
            Vector4 toReturn;
            Vector4.Normalize(ref v, out toReturn);
            return toReturn;
        }

        /// <summary>
        /// Normalizes the given vector.
        /// </summary>
        /// <param name="v">Vector to normalize.</param>
        /// <param name="result">Normalized vector.</param>
        public static void Normalize(ref Vector4 v, out Vector4 result)
        {
            Fix64 inverse = F64.C1.Div(Fix64.Sqrt((((v.X.Mul(v.X)).Add(v.Y.Mul(v.Y))).Add(v.Z.Mul(v.Z))).Add(v.W.Mul(v.W))));
            result.X = v.X.Mul(inverse);
            result.Y = v.Y.Mul(inverse);
            result.Z = v.Z.Mul(inverse);
            result.W = v.W.Mul(inverse);
        }

        /// <summary>
        /// Negates a vector.
        /// </summary>
        /// <param name="v">Vector to negate.</param>
        /// <param name="negated">Negated vector.</param>
        public static void Negate(ref Vector4 v, out Vector4 negated)
        {
            negated.X = v.X.Neg();
            negated.Y = v.Y.Neg();
            negated.Z = v.Z.Neg();
            negated.W = v.W.Neg();
        }


        /// <summary>
        /// Computes the absolute value of the input vector.
        /// </summary>
        /// <param name="v">Vector to take the absolute value of.</param>
        /// <param name="result">Vector with nonnegative elements.</param>
        public static void Abs(ref Vector4 v, out Vector4 result)
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
            if (v.W < F64.C0)
                result.W = v.W.Neg();
            else
                result.W = v.W;
        }

        /// <summary>
        /// Computes the absolute value of the input vector.
        /// </summary>
        /// <param name="v">Vector to take the absolute value of.</param>
        /// <returns>Vector with nonnegative elements.</returns>
        public static Vector4 Abs(Vector4 v)
        {
            Vector4 result;
            Abs(ref v, out result);
            return result;
        }

        /// <summary>
        /// Creates a vector from the lesser values in each vector.
        /// </summary>
        /// <param name="a">First input vector to compare values from.</param>
        /// <param name="b">Second input vector to compare values from.</param>
        /// <param name="min">Vector containing the lesser values of each vector.</param>
        public static void Min(ref Vector4 a, ref Vector4 b, out Vector4 min)
        {
            min.X = a.X < b.X ? a.X : b.X;
            min.Y = a.Y < b.Y ? a.Y : b.Y;
            min.Z = a.Z < b.Z ? a.Z : b.Z;
            min.W = a.W < b.W ? a.W : b.W;
        }

        /// <summary>
        /// Creates a vector from the lesser values in each vector.
        /// </summary>
        /// <param name="a">First input vector to compare values from.</param>
        /// <param name="b">Second input vector to compare values from.</param>
        /// <returns>Vector containing the lesser values of each vector.</returns>
        public static Vector4 Min(Vector4 a, Vector4 b)
        {
            Vector4 result;
            Min(ref a, ref b, out result);
            return result;
        }


        /// <summary>
        /// Creates a vector from the greater values in each vector.
        /// </summary>
        /// <param name="a">First input vector to compare values from.</param>
        /// <param name="b">Second input vector to compare values from.</param>
        /// <param name="max">Vector containing the greater values of each vector.</param>
        public static void Max(ref Vector4 a, ref Vector4 b, out Vector4 max)
        {
            max.X = a.X > b.X ? a.X : b.X;
            max.Y = a.Y > b.Y ? a.Y : b.Y;
            max.Z = a.Z > b.Z ? a.Z : b.Z;
            max.W = a.W > b.W ? a.W : b.W;
        }

        /// <summary>
        /// Creates a vector from the greater values in each vector.
        /// </summary>
        /// <param name="a">First input vector to compare values from.</param>
        /// <param name="b">Second input vector to compare values from.</param>
        /// <returns>Vector containing the greater values of each vector.</returns>
        public static Vector4 Max(Vector4 a, Vector4 b)
        {
            Vector4 result;
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
        public static Vector4 Lerp(Vector4 start, Vector4 end, Fix64 interpolationAmount)
        {
            Vector4 toReturn;
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
        public static void Lerp(ref Vector4 start, ref Vector4 end, Fix64 interpolationAmount, out Vector4 result)
        {
            Fix64 startAmount = F64.C1.Sub(interpolationAmount);
            result.X = (start.X.Mul(startAmount)).Add(end.X.Mul(interpolationAmount));
            result.Y = (start.Y.Mul(startAmount)).Add(end.Y.Mul(interpolationAmount));
            result.Z = (start.Z.Mul(startAmount)).Add(end.Z.Mul(interpolationAmount));
            result.W = (start.W.Mul(startAmount)).Add(end.W.Mul(interpolationAmount));
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
        public static void Hermite(ref Vector4 value1, ref Vector4 tangent1, ref Vector4 value2, ref Vector4 tangent2, Fix64 interpolationAmount, out Vector4 result)
        {
            Fix64 weightSquared = interpolationAmount.Mul(interpolationAmount);
            Fix64 weightCubed = interpolationAmount.Mul(weightSquared);
            Fix64 value1Blend = ((F64.C2.Mul(weightCubed)).Sub(F64.C3.Mul(weightSquared))).Add(F64.C1);
            Fix64 tangent1Blend = (weightCubed.Sub(F64.C2.Mul(weightSquared))).Add(interpolationAmount);
            Fix64 value2Blend = ((F64.C2.Neg()).Mul(weightCubed)).Add(F64.C3.Mul(weightSquared));
            Fix64 tangent2Blend = weightCubed.Sub(weightSquared);
            result.X = (((value1.X.Mul(value1Blend)).Add(value2.X.Mul(value2Blend))).Add(tangent1.X.Mul(tangent1Blend))).Add(tangent2.X.Mul(tangent2Blend));
            result.Y = (((value1.Y.Mul(value1Blend)).Add(value2.Y.Mul(value2Blend))).Add(tangent1.Y.Mul(tangent1Blend))).Add(tangent2.Y.Mul(tangent2Blend));
            result.Z = (((value1.Z.Mul(value1Blend)).Add(value2.Z.Mul(value2Blend))).Add(tangent1.Z.Mul(tangent1Blend))).Add(tangent2.Z.Mul(tangent2Blend));
            result.W = (((value1.W.Mul(value1Blend)).Add(value2.W.Mul(value2Blend))).Add(tangent1.W.Mul(tangent1Blend))).Add(tangent2.W.Mul(tangent2Blend));
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
        public static Vector4 Hermite(Vector4 value1, Vector4 tangent1, Vector4 value2, Vector4 tangent2, Fix64 interpolationAmount)
        {
            Vector4 toReturn;
            Hermite(ref value1, ref tangent1, ref value2, ref tangent2, interpolationAmount, out toReturn);
            return toReturn;
        }
    }
}
