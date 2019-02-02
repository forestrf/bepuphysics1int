﻿using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUphysics.Entities;
using BEPUphysics.CollisionRuleManagement;
using BEPUutilities;
using BEPUphysics.Materials;

namespace BEPUphysics.Vehicle
{
    /// <summary>
    /// Uses a cylinder cast as the shape of a wheel.
    /// </summary>
    public class CylinderCastWheelShape : WheelShape
    {
        private CylinderShape shape;

        private Quaternion localWheelOrientation;
        /// <summary>
        /// Gets or sets the unsteered orientation of the wheel in the vehicle's local space.
        /// </summary>
        public Quaternion LocalWheelOrientation
        {
            get { return localWheelOrientation; }
            set { localWheelOrientation = value; }
        }

        /// <summary>
        /// Creates a new cylinder cast based wheel shape.
        /// </summary>
        /// <param name="radius">Radius of the wheel.</param>
        /// <param name="width">Width of the wheel.</param>
        /// <param name="localWheelOrientation">Unsteered orientation of the wheel in the vehicle's local space.</param>
        /// <param name="localGraphicTransform">Local graphic transform of the wheel shape.
        /// This transform is applied first when creating the shape's worldTransform.</param>
        /// <param name="includeSteeringTransformInCast">Whether or not to include the steering transform in the wheel shape cast. If false, the casted wheel shape will always point straight forward.
        /// If true, it will rotate with steering. Sometimes, setting this to false is helpful when the cast shape would otherwise become exposed when steering.</param>
        public CylinderCastWheelShape(Fix32 radius, Fix32 width, Quaternion localWheelOrientation, Matrix localGraphicTransform, bool includeSteeringTransformInCast)
        {
            shape = new CylinderShape(width, radius);
            this.LocalWheelOrientation = localWheelOrientation;
            LocalGraphicTransform = localGraphicTransform;
            this.IncludeSteeringTransformInCast = includeSteeringTransformInCast;
        }

        /// <summary>
        /// Gets or sets the radius of the wheel.
        /// </summary>
        public override sealed Fix32 Radius
        {
            get { return shape.Radius; }
            set
            {
                shape.Radius = MathHelper.Max(value, Fix32.Zero);
                Initialize();
            }
        }

        /// <summary>
        /// Gets or sets the width of the wheel.
        /// </summary>
        public Fix32 Width
        {
            get { return shape.Height; }
            set
            {
                shape.Height = MathHelper.Max(value, Fix32.Zero);
                Initialize();
            }
        }

        /// <summary>
        /// Gets or sets whether or not to include the rotation from steering in the cast. If false, the casted wheel shape will always point straight forward.
        /// If true, it will rotate with steering. Sometimes, setting this to false is helpful when the cast shape would otherwise become exposed when steering.
        /// </summary>
        public bool IncludeSteeringTransformInCast { get; set; }

        /// <summary>
        /// Updates the wheel's world transform for graphics.
        /// Called automatically by the owning wheel at the end of each frame.
        /// If the engine is updating asynchronously, you can call this inside of a space read buffer lock
        /// and update the wheel transforms safely.
        /// </summary>
        public override void UpdateWorldTransform()
        {
#if !WINDOWS
            Vector3 newPosition = new Vector3();
#else
            Vector3 newPosition;
#endif
            Vector3 worldAttachmentPoint;
            Vector3 localAttach;
            Vector3.Add(ref wheel.suspension.localAttachmentPoint, ref wheel.vehicle.Body.CollisionInformation.localPosition, out localAttach);
            worldTransform = Matrix3x3.ToMatrix4X4(wheel.vehicle.Body.BufferedStates.InterpolatedStates.OrientationMatrix);

            Matrix.TransformNormal(ref localAttach, ref worldTransform, out worldAttachmentPoint);
            worldAttachmentPoint = worldAttachmentPoint + (wheel.vehicle.Body.BufferedStates.InterpolatedStates.Position);

            Vector3 worldDirection;
            Matrix.Transform(ref wheel.suspension.localDirection, ref worldTransform, out worldDirection);

            Fix32 length = wheel.suspension.currentLength;
            newPosition.X = worldAttachmentPoint.X .Add (worldDirection.X .Mul (length));
            newPosition.Y = worldAttachmentPoint.Y .Add (worldDirection.Y .Mul (length));
            newPosition.Z = worldAttachmentPoint.Z .Add (worldDirection.Z .Mul (length));

            Matrix spinTransform;

            Vector3 localSpinAxis;
            Quaternion.Transform(ref Toolbox.UpVector, ref localWheelOrientation, out localSpinAxis);
            Matrix.CreateFromAxisAngle(ref localSpinAxis, spinAngle, out spinTransform);


            Matrix localTurnTransform;
            Matrix.Multiply(ref localGraphicTransform, ref spinTransform, out localTurnTransform);
            Matrix.Multiply(ref localTurnTransform, ref steeringTransform, out localTurnTransform);
            //Matrix.Multiply(ref localTurnTransform, ref spinTransform, out localTurnTransform);
            Matrix.Multiply(ref localTurnTransform, ref worldTransform, out worldTransform);
			worldTransform.Translation = worldTransform.Translation + (newPosition);
		}

        /// <summary>
        /// Finds a supporting entity, the contact location, and the contact normal.
        /// </summary>
        /// <param name="location">Contact point between the wheel and the support.</param>
        /// <param name="normal">Contact normal between the wheel and the support.</param>
        /// <param name="suspensionLength">Length of the suspension at the contact.</param>
        /// <param name="supportingCollidable">Collidable supporting the wheel, if any.</param>
        /// <param name="entity">Supporting object.</param>
        /// <param name="material">Material of the wheel.</param>
        /// <returns>Whether or not any support was found.</returns>
        protected internal override bool FindSupport(out Vector3 location, out Vector3 normal, out Fix32 suspensionLength, out Collidable supportingCollidable, out Entity entity, out Material material)
        {
            suspensionLength = Fix32.MaxValue;
            location = Toolbox.NoVector;
            supportingCollidable = null;
            entity = null;
            normal = Toolbox.NoVector;
            material = null;

            Collidable testCollidable;
            RayHit rayHit;

            bool hit = false;

            Quaternion localSteeringTransform;
            Quaternion.CreateFromAxisAngle(ref wheel.suspension.localDirection, steeringAngle, out localSteeringTransform);
            var startingTransform = new RigidTransform
            {
                Position = wheel.suspension.worldAttachmentPoint,
                Orientation = Quaternion.Concatenate(Quaternion.Concatenate(LocalWheelOrientation, IncludeSteeringTransformInCast ? localSteeringTransform : Quaternion.Identity), wheel.vehicle.Body.orientation)
            };
            Vector3 sweep;
            Vector3.Multiply(ref wheel.suspension.worldDirection, wheel.suspension.restLength, out sweep);

            for (int i = 0; i < detector.CollisionInformation.pairs.Count; i++)
            {
                var pair = detector.CollisionInformation.pairs[i];
                testCollidable = (pair.BroadPhaseOverlap.entryA == detector.CollisionInformation ? pair.BroadPhaseOverlap.entryB : pair.BroadPhaseOverlap.entryA) as Collidable;
                if (testCollidable != null)
                {
                    if (CollisionRules.CollisionRuleCalculator(this, testCollidable) == CollisionRule.Normal &&
                        testCollidable.ConvexCast(shape, ref startingTransform, ref sweep, out rayHit) &&
                        rayHit.T .Mul (wheel.suspension.restLength) < suspensionLength)
                    {
                        suspensionLength = rayHit.T .Mul (wheel.suspension.restLength);
                        EntityCollidable entityCollidable;
                        if ((entityCollidable = testCollidable as EntityCollidable) != null)
                        {
                            entity = entityCollidable.Entity;
                            material = entityCollidable.Entity.Material;
                        }
                        else
                        {
                            entity = null;
                            supportingCollidable = testCollidable;
                            var materialOwner = testCollidable as IMaterialOwner;
                            if (materialOwner != null)
                                material = materialOwner.Material;
                        }
                        location = rayHit.Location;
                        normal = rayHit.Normal;
                        hit = true;
                    }
                }
            }
            if (hit)
            {
                if (suspensionLength > Fix32.Zero)
                {
                    Fix32 dot;
                    Vector3.Dot(ref normal, ref wheel.suspension.worldDirection, out dot);
                    if (dot > Fix32.Zero)
                    {
                        //The cylinder cast produced a normal which is opposite of what we expect.
                        Vector3.Negate(ref normal, out normal);
                    }
                    normal.Normalize();
                }
                else
                    Vector3.Negate(ref wheel.suspension.worldDirection, out normal);
                return true;
            }
            return false;
        }


        /// <summary>
        /// Initializes the detector entity and any other necessary logic.
        /// </summary>
        protected internal override void Initialize()
        {
            //Setup the dimensions of the detector.
            var initialTransform = new RigidTransform { Orientation = LocalWheelOrientation };
            BoundingBox boundingBox;
            shape.GetBoundingBox(ref initialTransform, out boundingBox);
            var expansion = wheel.suspension.localDirection * wheel.suspension.restLength;
            if (expansion.X > Fix32.Zero)
                boundingBox.Max.X = boundingBox.Max.X .Add (expansion.X);
            else if (expansion.X < Fix32.Zero)
                boundingBox.Min.X = boundingBox.Min.X.Add(expansion.X);

            if (expansion.Y > Fix32.Zero)
                boundingBox.Max.Y = boundingBox.Max.Y.Add(expansion.Y);
            else if (expansion.Y < Fix32.Zero)
                boundingBox.Min.Y = boundingBox.Min.Y.Add(expansion.Y);

            if (expansion.Z > Fix32.Zero)
                boundingBox.Max.Z = boundingBox.Max.Z.Add(expansion.Z);
            else if (expansion.Z < Fix32.Zero)
                boundingBox.Min.Z = boundingBox.Min.Z.Add(expansion.Z);


            detector.Width  = boundingBox.Max.X .Sub (boundingBox.Min.X);
            detector.Height = boundingBox.Max.Y .Sub (boundingBox.Min.Y);
            detector.Length = boundingBox.Max.Z .Sub (boundingBox.Min.Z);
        }

        /// <summary>
        /// Updates the position of the detector before each step.
        /// </summary>
        protected internal override void UpdateDetectorPosition()
        {
#if !WINDOWS
            Vector3 newPosition = new Vector3();
#else
            Vector3 newPosition;
#endif

            newPosition.X = wheel.suspension.worldAttachmentPoint.X .Add (wheel.suspension.worldDirection.X .Mul (wheel.suspension.restLength) .Mul (F64.C0p5));
            newPosition.Y = wheel.suspension.worldAttachmentPoint.Y .Add (wheel.suspension.worldDirection.Y .Mul (wheel.suspension.restLength) .Mul (F64.C0p5));
            newPosition.Z = wheel.suspension.worldAttachmentPoint.Z .Add (wheel.suspension.worldDirection.Z .Mul (wheel.suspension.restLength) .Mul (F64.C0p5));

            detector.Position = newPosition;
            if (IncludeSteeringTransformInCast)
            {
                Quaternion localSteeringTransform;
                Quaternion.CreateFromAxisAngle(ref wheel.suspension.localDirection, steeringAngle, out localSteeringTransform);

                detector.Orientation = Quaternion.Concatenate(localSteeringTransform, wheel.Vehicle.Body.orientation);
            }
            else
            {
                detector.Orientation = wheel.Vehicle.Body.orientation;
            }
            Vector3 linearVelocity;
            Vector3.Subtract(ref newPosition, ref wheel.vehicle.Body.position, out linearVelocity);
            Vector3.Cross(ref linearVelocity, ref wheel.vehicle.Body.angularVelocity, out linearVelocity);
            Vector3.Add(ref linearVelocity, ref wheel.vehicle.Body.linearVelocity, out linearVelocity);
            detector.LinearVelocity = linearVelocity;
            detector.AngularVelocity = wheel.vehicle.Body.angularVelocity;
        }
    }
}