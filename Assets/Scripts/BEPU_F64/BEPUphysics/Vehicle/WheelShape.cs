﻿using System;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;

using BEPUphysics.CollisionRuleManagement;
using BEPUphysics.Materials;
using BEPUutilities;


namespace BEPUphysics.Vehicle
{
    /// <summary>
    /// Superclass for the shape of the tires of a vehicle.
    /// Responsible for figuring out where the wheel touches the ground and
    /// managing graphical properties.
    /// </summary>
    public abstract class WheelShape : ICollisionRulesOwner
    {
        private Fix32 airborneWheelAcceleration = 40.ToFix32();


        private Fix32 airborneWheelDeceleration = 4.ToFix32();
        private Fix32 brakeFreezeWheelDeceleration = 40.ToFix32();

        /// <summary>
        /// Collects collision pairs from the environment.
        /// </summary>
        protected internal Box detector = new Box(Vector3.Zero, Fix32.Zero, Fix32.Zero, Fix32.Zero);

        protected internal Matrix localGraphicTransform;
        protected Fix32 spinAngle;


        protected Fix32 spinVelocity;
        internal Fix32 steeringAngle;

        internal Matrix steeringTransform;
        protected internal Wheel wheel;

        protected internal Matrix worldTransform;

        CollisionRules collisionRules = new CollisionRules() { Group = CollisionRules.DefaultDynamicCollisionGroup };
        /// <summary>
        /// Gets or sets the collision rules used by the wheel.
        /// </summary>
        public CollisionRules CollisionRules
        {
            get { return collisionRules; }
            set { collisionRules = value; }
        }

        /// <summary>
        /// Gets or sets the graphical radius of the wheel.
        /// </summary>
        public abstract Fix32 Radius { get; set; }

        /// <summary>
        /// Gets or sets the rate at which the wheel's spinning velocity increases when accelerating and airborne.
        /// This is a purely graphical effect.
        /// </summary>
        public Fix32 AirborneWheelAcceleration
        {
            get { return airborneWheelAcceleration; }
            set { airborneWheelAcceleration = value.Abs(); }
        }

        /// <summary>
        /// Gets or sets the rate at which the wheel's spinning velocity decreases when the wheel is airborne and its motor is idle.
        /// This is a purely graphical effect.
        /// </summary>
        public Fix32 AirborneWheelDeceleration
        {
            get { return airborneWheelDeceleration; }
            set { airborneWheelDeceleration = value.Abs(); }
        }

        /// <summary>
        /// Gets or sets the rate at which the wheel's spinning velocity decreases when braking.
        /// This is a purely graphical effect.
        /// </summary>
        public Fix32 BrakeFreezeWheelDeceleration
        {
            get { return brakeFreezeWheelDeceleration; }
            set { brakeFreezeWheelDeceleration = value.Abs(); }
        }

        /// <summary>
        /// Gets the detector entity used by the wheelshape to collect collision pairs.
        /// </summary>
        public Box Detector
        {
            get { return detector; }
        }

        /// <summary>
        /// Gets or sets whether or not to halt the wheel spin while the WheelBrake is active.
        /// </summary>
        public bool FreezeWheelsWhileBraking { get; set; }

        /// <summary>
        /// Gets or sets the local graphic transform of the wheel shape.
        /// This transform is applied first when creating the shape's worldTransform.
        /// </summary>
        public Matrix LocalGraphicTransform
        {
            get { return localGraphicTransform; }
            set { localGraphicTransform = value; }
        }

        /// <summary>
        /// Gets or sets the current spin angle of this wheel.
        /// This changes each frame based on the relative velocity between the
        /// support and the wheel.
        /// </summary>
        public Fix32 SpinAngle
        {
            get { return spinAngle; }
            set { spinAngle = value; }
        }

        /// <summary>
        /// Gets or sets the graphical spin velocity of the wheel based on the relative velocity 
        /// between the support and the wheel.  Whenever the wheel is in contact with
        /// the ground, the spin velocity will be each frame.
        /// </summary>
        public Fix32 SpinVelocity
        {
            get { return spinVelocity; }
            set { spinVelocity = value; }
        }

        /// <summary>
        /// Gets or sets the current steering angle of this wheel.
        /// </summary>
        public Fix32 SteeringAngle
        {
            get { return steeringAngle; }
            set { steeringAngle = value; }
        }

        /// <summary>
        /// Gets the wheel object associated with this shape.
        /// </summary>
        public Wheel Wheel
        {
            get { return wheel; }
            internal set { wheel = value; }
        }

        /// <summary>
        /// Gets the world matrix of the wheel for positioning a graphic.
        /// </summary>
        public Matrix WorldTransform
        {
            get { return worldTransform; }
        }


        /// <summary>
        /// Updates the wheel's world transform for graphics.
        /// Called automatically by the owning wheel at the end of each frame.
        /// If the engine is updating asynchronously, you can call this inside of a space read buffer lock
        /// and update the wheel transforms safely.
        /// </summary>
        public abstract void UpdateWorldTransform();


        internal void OnAdditionToSpace(Space space)
        {
            detector.CollisionInformation.collisionRules.Specific.Add(wheel.vehicle.Body.CollisionInformation.collisionRules, CollisionRule.NoBroadPhase);
            detector.CollisionInformation.collisionRules.Personal = CollisionRule.NoNarrowPhaseUpdate;
            detector.CollisionInformation.collisionRules.group = CollisionRules.DefaultDynamicCollisionGroup;
            //Need to put the detectors in appropriate locations before adding, or else the broad phase would see objects at (0,0,0) and make things gross.
            UpdateDetectorPosition();
            space.Add(detector);

        }

        internal void OnRemovalFromSpace(Space space)
        {
            space.Remove(detector);
            detector.CollisionInformation.CollisionRules.Specific.Remove(wheel.vehicle.Body.CollisionInformation.collisionRules);
        }

        /// <summary>
        /// Updates the spin velocity and spin angle for the shape.
        /// </summary>
        /// <param name="dt">Simulation timestep.</param>
        internal void UpdateSpin(Fix32 dt)
        {
            if (wheel.HasSupport && !(wheel.brake.IsBraking && FreezeWheelsWhileBraking))
            {
                //On the ground, not braking.
                spinVelocity = wheel.drivingMotor.RelativeVelocity .Div (Radius);
            }
            else if (wheel.HasSupport && wheel.brake.IsBraking && FreezeWheelsWhileBraking)
            {
                //On the ground, braking
                Fix32 deceleratedValue = Fix32.Zero;
                if (spinVelocity > Fix32.Zero)
                    deceleratedValue = MathHelper.Max(spinVelocity .Sub (brakeFreezeWheelDeceleration .Mul (dt)), Fix32.Zero);
                else if (spinVelocity < Fix32.Zero)
                    deceleratedValue = MathHelper.Min(spinVelocity .Add (brakeFreezeWheelDeceleration .Mul (dt)), Fix32.Zero);

                spinVelocity = wheel.drivingMotor.RelativeVelocity .Div (Radius);

                if (deceleratedValue.Abs() < spinVelocity.Abs())
                    spinVelocity = deceleratedValue;
            }
            else if (!wheel.HasSupport && wheel.drivingMotor.TargetSpeed != Fix32.Zero)
            {
                //Airborne and accelerating, increase spin velocity.
                Fix32 maxSpeed = (wheel.drivingMotor.TargetSpeed).Abs() .Div (Radius);
                spinVelocity = MathHelper.Clamp(spinVelocity .Add (wheel.drivingMotor.TargetSpeed.Sign() .Mul (airborneWheelAcceleration) .Mul (dt)), maxSpeed.Neg(), maxSpeed);
            }
            else if (!wheel.HasSupport && wheel.Brake.IsBraking)
            {
                //Airborne and braking
                if (spinVelocity > Fix32.Zero)
                    spinVelocity = MathHelper.Max(spinVelocity .Sub (brakeFreezeWheelDeceleration .Mul (dt)), Fix32.Zero);
                else if (spinVelocity < Fix32.Zero)
                    spinVelocity = MathHelper.Min(spinVelocity .Add (brakeFreezeWheelDeceleration .Mul (dt)), Fix32.Zero);
            }
            else if (!wheel.HasSupport)
            {
                //Just idly slowing down.
                if (spinVelocity > Fix32.Zero)
                    spinVelocity = MathHelper.Max(spinVelocity .Sub (airborneWheelDeceleration .Mul (dt)), Fix32.Zero);
                else if (spinVelocity < Fix32.Zero)
                    spinVelocity = MathHelper.Min(spinVelocity .Add (airborneWheelDeceleration .Mul (dt)), Fix32.Zero);
            }
            spinAngle = spinAngle .Add (spinVelocity .Mul (dt));
        }

        /// <summary>
        /// Finds a supporting entity, the contact location, and the contact normal.
        /// </summary>
        /// <param name="location">Contact point between the wheel and the support.</param>
        /// <param name="normal">Contact normal between the wheel and the support.</param>
        /// <param name="suspensionLength">Length of the suspension at the contact.</param>
        /// <param name="supportCollidable">Collidable supporting the wheel, if any.</param>
        /// <param name="entity">Entity supporting the wheel, if any.</param>
        /// <param name="material">Material of the support.</param>
        /// <returns>Whether or not any support was found.</returns>
        protected internal abstract bool FindSupport(out Vector3 location, out Vector3 normal, out Fix32 suspensionLength, out Collidable supportCollidable, out Entity entity, out Material material);

        /// <summary>
        /// Initializes the detector entity and any other necessary logic.
        /// </summary>
        protected internal abstract void Initialize();

        /// <summary>
        /// Updates the position of the detector before each step.
        /// </summary>
        protected internal abstract void UpdateDetectorPosition();

    }
}