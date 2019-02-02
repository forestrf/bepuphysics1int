﻿namespace BEPUutilities
{
    /// <summary>
    /// Provides XNA-like ray functionality.
    /// </summary>
    public struct Ray
    {
        /// <summary>
        /// Starting position of the ray.
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// Direction in which the ray points.
        /// </summary>
        public Vector3 Direction;


        /// <summary>
        /// Constructs a new ray.
        /// </summary>
        /// <param name="position">Starting position of the ray.</param>
        /// <param name="direction">Direction in which the ray points.</param>
        public Ray(Vector3 position, Vector3 direction)
        {
            this.Position = position;
            this.Direction = direction;
        }



        /// <summary>
        /// Determines if and when the ray intersects the bounding box.
        /// </summary>
        /// <param name="boundingBox">Bounding box to test against.</param>
        /// <param name="t">The length along the ray to the impact, if any impact occurs.</param>
        /// <returns>True if the ray intersects the target, false otherwise.</returns>
        public bool Intersects(ref BoundingBox boundingBox, out Fix32 t)
        {
			Fix32 tmin = Fix32.Zero, tmax = Fix32.MaxValue;
            if (Direction.X.Abs() < Toolbox.Epsilon)
            {
                if (Position.X < boundingBox.Min.X || Position.X > boundingBox.Max.X)
                {
                    //If the ray isn't pointing along the axis at all, and is outside of the box's interval, then it
                    //can't be intersecting.
                    t = Fix32.Zero;
                    return false;
                }
            }
            else
            {
                var inverseDirection = Fix32.One.Div(Direction.X);
                var t1 = (boundingBox.Min.X.Sub(Position.X)).Mul(inverseDirection);
                var t2 = (boundingBox.Max.X.Sub(Position.X)).Mul(inverseDirection);
                if (t1 > t2)
                {
					Fix32 temp = t1;
                    t1 = t2;
                    t2 = temp;
                }
                tmin = MathHelper.Max(tmin, t1);
                tmax = MathHelper.Min(tmax, t2);
                if (tmin > tmax)
                {
                    t = Fix32.Zero;
                    return false;
                }
            }
            if (Direction.Y.Abs() < Toolbox.Epsilon)
            {
                if (Position.Y < boundingBox.Min.Y || Position.Y > boundingBox.Max.Y)
                {
                    //If the ray isn't pointing along the axis at all, and is outside of the box's interval, then it
                    //can't be intersecting.
                    t = Fix32.Zero;
                    return false;
                }
            }
            else
            {
                var inverseDirection = Fix32.One.Div(Direction.Y);
                var t1 = (boundingBox.Min.Y.Sub(Position.Y)).Mul(inverseDirection);
                var t2 = (boundingBox.Max.Y.Sub(Position.Y)).Mul(inverseDirection);
                if (t1 > t2)
                {
					Fix32 temp = t1;
                    t1 = t2;
                    t2 = temp;
                }
                tmin = MathHelper.Max(tmin, t1);
                tmax = MathHelper.Min(tmax, t2);
                if (tmin > tmax)
                {
                    t = Fix32.Zero;
                    return false;
                }
            }
            if (Direction.Z.Abs() < Toolbox.Epsilon)
            {
                if (Position.Z < boundingBox.Min.Z || Position.Z > boundingBox.Max.Z)
                {
                    //If the ray isn't pointing along the axis at all, and is outside of the box's interval, then it
                    //can't be intersecting.
                    t = Fix32.Zero;
                    return false;
                }
            }
            else
            {
                var inverseDirection = Fix32.One.Div(Direction.Z);
                var t1 = (boundingBox.Min.Z.Sub(Position.Z)).Mul(inverseDirection);
                var t2 = (boundingBox.Max.Z.Sub(Position.Z)).Mul(inverseDirection);
                if (t1 > t2)
                {
					Fix32 temp = t1;
                    t1 = t2;
                    t2 = temp;
                }
                tmin = MathHelper.Max(tmin, t1);
                tmax = MathHelper.Min(tmax, t2);
                if (tmin > tmax)
                {
                    t = Fix32.Zero;
                    return false;
                }
            }
            t = tmin;
            return true;
        }

        /// <summary>
        /// Determines if and when the ray intersects the bounding box.
        /// </summary>
        /// <param name="boundingBox">Bounding box to test against.</param>
        /// <param name="t">The length along the ray to the impact, if any impact occurs.</param>
        /// <returns>True if the ray intersects the target, false otherwise.</returns>
        public bool Intersects(BoundingBox boundingBox, out Fix32 t)
        {
            return Intersects(ref boundingBox, out t);
        }

        /// <summary>
        /// Determines if and when the ray intersects the plane.
        /// </summary>
        /// <param name="plane">Plane to test against.</param>
        /// <param name="t">The length along the ray to the impact, if any impact occurs.</param>
        /// <returns>True if the ray intersects the target, false otherwise.</returns>
        public bool Intersects(ref Plane plane, out Fix32 t)
        {
			Fix32 velocity;
            Vector3.Dot(ref Direction, ref plane.Normal, out velocity);
            if (velocity.Abs() < Toolbox.Epsilon)
            {
                t = Fix32.Zero;
                return false;
            }
			Fix32 distanceAlongNormal;
            Vector3.Dot(ref Position, ref plane.Normal, out distanceAlongNormal);
            distanceAlongNormal = distanceAlongNormal.Add(plane.D);
            t = distanceAlongNormal.Neg().Div(velocity);
            return t >= Toolbox.MinusEpsilon;
        }

        /// <summary>
        /// Determines if and when the ray intersects the plane.
        /// </summary>
        /// <param name="plane">Plane to test against.</param>
        /// <param name="t">The length along the ray to the impact, if any impact occurs.</param>
        /// <returns>True if the ray intersects the target, false otherwise.</returns>
        public bool Intersects(Plane plane, out Fix32 t)
        {
            return Intersects(ref plane, out t);
        }

        /// <summary>
        /// Computes a point along a ray given the length along the ray from the ray position.
        /// </summary>
        /// <param name="t">Length along the ray from the ray position in terms of the ray's direction.</param>
        /// <param name="v">Point along the ray at the given location.</param>
        public void GetPointOnRay(Fix32 t, out Vector3 v)
        {
            Vector3.Multiply(ref Direction, t, out v);
            Vector3.Add(ref v, ref Position, out v);
        }
    }
}
