﻿using BEPUphysics.Constraints;
using BEPUphysics.Entities;
 
using BEPUphysics.Materials;
using BEPUutilities;


namespace BEPUphysics.Vehicle
{
    /// <summary>
    /// Handles a wheel's driving force for a vehicle.
    /// </summary>
    public class WheelDrivingMotor : ISolverSettings
    {
        #region Static Stuff

        /// <summary>
        /// Default blender used by WheelSlidingFriction constraints.
        /// </summary>
        public static WheelFrictionBlender DefaultGripFrictionBlender;

        static WheelDrivingMotor()
        {
            DefaultGripFrictionBlender = BlendFriction;
        }

        /// <summary>
        /// Function which takes the friction values from a wheel and a supporting material and computes the blended friction.
        /// </summary>
        /// <param name="wheelFriction">Friction coefficient associated with the wheel.</param>
        /// <param name="materialFriction">Friction coefficient associated with the support material.</param>
        /// <param name="usingKineticFriction">True if the friction coefficients passed into the blender are kinetic coefficients, false otherwise.</param>
        /// <param name="wheel">Wheel being blended.</param>
        /// <returns>Blended friction coefficient.</returns>
        public static Fix32 BlendFriction(Fix32 wheelFriction, Fix32 materialFriction, bool usingKineticFriction, Wheel wheel)
        {
            return wheelFriction.Mul(materialFriction);
        }

        #endregion

        internal Fix32 accumulatedImpulse;

        //Fix64 linearBX, linearBY, linearBZ;
        internal Fix32 angularAX, angularAY, angularAZ;
        internal Fix32 angularBX, angularBY, angularBZ;
        internal bool isActive = true;
        internal Fix32 linearAX, linearAY, linearAZ;
        private Fix32 currentFrictionCoefficient;
        internal Vector3 forceAxis;
        private Fix32 gripFriction;
        private WheelFrictionBlender gripFrictionBlender = DefaultGripFrictionBlender;
        private Fix32 maxMotorForceDt;
        private Fix32 maximumBackwardForce = Fix32.MaxValue;
        private Fix32 maximumForwardForce = Fix32.MaxValue;
        internal SolverSettings solverSettings = new SolverSettings();
        private Fix32 targetSpeed;
        private Wheel wheel;
        internal int numIterationsAtZeroImpulse;
        private Entity vehicleEntity, supportEntity;

        //Inverse effective mass matrix
        internal Fix32 velocityToImpulse;
        private bool supportIsDynamic;

        /// <summary>
        /// Constructs a new wheel motor.
        /// </summary>
        /// <param name="gripFriction">Friction coefficient of the wheel.  Blended with the ground's friction coefficient and normal force to determine a maximum force.</param>
        /// <param name="maximumForwardForce">Maximum force that the wheel motor can apply when driving forward (a target speed greater than zero).</param>
        /// <param name="maximumBackwardForce">Maximum force that the wheel motor can apply when driving backward (a target speed less than zero).</param>
        public WheelDrivingMotor(Fix32 gripFriction, Fix32 maximumForwardForce, Fix32 maximumBackwardForce)
        {
            GripFriction = gripFriction;
            MaximumForwardForce = maximumForwardForce;
            MaximumBackwardForce = maximumBackwardForce;
        }

        internal WheelDrivingMotor(Wheel wheel)
        {
            Wheel = wheel;
        }

        /// <summary>
        /// Gets the coefficient of grip friction between the wheel and support.
        /// This coefficient is the blended result of the supporting entity's friction and the wheel's friction.
        /// </summary>
        public Fix32 BlendedCoefficient
        {
            get { return currentFrictionCoefficient; }
        }

        /// <summary>
        /// Gets the axis along which the driving forces are applied.
        /// </summary>
        public Vector3 ForceAxis
        {
            get { return forceAxis; }
        }

        /// <summary>
        /// Gets or sets the coefficient of forward-backward gripping friction for this wheel.
        /// This coefficient and the supporting entity's coefficient of friction will be 
        /// taken into account to determine the used coefficient at any given time.
        /// </summary>
        public Fix32 GripFriction
        {
            get { return gripFriction; }
            set { gripFriction = MathHelper.Max(value, F64.C0); }
        }

        /// <summary>
        /// Gets or sets the function used to blend the supporting entity's friction and the wheel's friction.
        /// </summary>
        public WheelFrictionBlender GripFrictionBlender
        {
            get { return gripFrictionBlender; }
            set { gripFrictionBlender = value; }
        }

        /// <summary>
        /// Gets or sets the maximum force that the wheel motor can apply when driving backward (a target speed less than zero).
        /// </summary>
        public Fix32 MaximumBackwardForce
        {
            get { return maximumBackwardForce; }
            set { maximumBackwardForce = value; }
        }

        /// <summary>
        /// Gets or sets the maximum force that the wheel motor can apply when driving forward (a target speed greater than zero).
        /// </summary>
        public Fix32 MaximumForwardForce
        {
            get { return maximumForwardForce; }
            set { maximumForwardForce = value; }
        }

        /// <summary>
        /// Gets or sets the target speed of this wheel.
        /// </summary>
        public Fix32 TargetSpeed
        {
            get { return targetSpeed; }
            set { targetSpeed = value; }
        }

        /// <summary>
        /// Gets the force this wheel's motor is applying.
        /// </summary>
        public Fix32 TotalImpulse
        {
            get { return accumulatedImpulse; }
        }

        /// <summary>
        /// Gets the wheel that this motor applies to.
        /// </summary>
        public Wheel Wheel
        {
            get { return wheel; }
            internal set { wheel = value; }
        }

        #region ISolverSettings Members

        /// <summary>
        /// Gets the solver settings used by this wheel constraint.
        /// </summary>
        public SolverSettings SolverSettings
        {
            get { return solverSettings; }
        }

        #endregion

        /// <summary>
        /// Gets the relative velocity between the ground and wheel.
        /// </summary>
        /// <returns>Relative velocity between the ground and wheel.</returns>
        public Fix32 RelativeVelocity
        {
            get
            {
                Fix32 velocity = F64.C0;
                if (vehicleEntity != null)
					velocity = velocity.Add((((((vehicleEntity.linearVelocity.X.Mul(linearAX)).Add(vehicleEntity.linearVelocity.Y.Mul(linearAY))).Add(vehicleEntity.linearVelocity.Z.Mul(linearAZ))).Add(vehicleEntity.angularVelocity.X.Mul(angularAX))).Add(vehicleEntity.angularVelocity.Y.Mul(angularAY))).Add(vehicleEntity.angularVelocity.Z.Mul(angularAZ)));
                if (supportEntity != null)
					velocity = velocity.Add((((((supportEntity.linearVelocity.X.Neg().Mul(linearAX)).Sub(supportEntity.linearVelocity.Y.Mul(linearAY))).Sub(supportEntity.linearVelocity.Z.Mul(linearAZ))).Add(supportEntity.angularVelocity.X.Mul(angularBX))).Add(supportEntity.angularVelocity.Y.Mul(angularBY))).Add(supportEntity.angularVelocity.Z.Mul(angularBZ)));
                return velocity;
            }
        }

        internal Fix32 ApplyImpulse()
        {
            //Compute relative velocity
            Fix32 lambda = (RelativeVelocity.Sub(targetSpeed)).Mul(velocityToImpulse); //convert to impulse


            //Clamp accumulated impulse
            Fix32 previousAccumulatedImpulse = accumulatedImpulse;
			accumulatedImpulse = accumulatedImpulse.Add(lambda);
            //Don't brake, and take into account the motor's maximum force.
            if (targetSpeed > F64.C0)
                accumulatedImpulse = MathHelper.Clamp(accumulatedImpulse, F64.C0, maxMotorForceDt); //MathHelper.Min(MathHelper.Max(accumulatedImpulse, 0), myMaxMotorForceDt);
            else if (targetSpeed < F64.C0)
                accumulatedImpulse = MathHelper.Clamp(accumulatedImpulse, maxMotorForceDt, F64.C0); //MathHelper.Max(MathHelper.Min(accumulatedImpulse, 0), myMaxMotorForceDt);
            else
                accumulatedImpulse = F64.C0;
            //Friction
            Fix32 maxForce = currentFrictionCoefficient.Mul(wheel.suspension.accumulatedImpulse);
            accumulatedImpulse = MathHelper.Clamp(accumulatedImpulse, maxForce, maxForce.Neg());
            lambda = accumulatedImpulse.Sub(previousAccumulatedImpulse);


            //Apply the impulse
#if !WINDOWS
            Vector3 linear = new Vector3();
            Vector3 angular = new Vector3();
#else
            Vector3 linear, angular;
#endif
            linear.X = lambda.Mul(linearAX);
            linear.Y = lambda.Mul(linearAY);
            linear.Z = lambda.Mul(linearAZ);
            if (vehicleEntity.isDynamic)
            {
                angular.X = lambda.Mul(angularAX);
                angular.Y = lambda.Mul(angularAY);
                angular.Z = lambda.Mul(angularAZ);
                vehicleEntity.ApplyLinearImpulse(ref linear);
                vehicleEntity.ApplyAngularImpulse(ref angular);
            }
            if (supportIsDynamic)
            {
                linear.X = linear.X.Neg();
                linear.Y = linear.Y.Neg();
                linear.Z = linear.Z.Neg();
                angular.X = lambda.Mul(angularBX);
                angular.Y = lambda.Mul(angularBY);
                angular.Z = lambda.Mul(angularBZ);
                supportEntity.ApplyLinearImpulse(ref linear);
                supportEntity.ApplyAngularImpulse(ref angular);
            }

            return lambda;
        }

        internal void PreStep(Fix32 dt)
        {
            vehicleEntity = wheel.Vehicle.Body;
            supportEntity = wheel.SupportingEntity;
            supportIsDynamic = supportEntity != null && supportEntity.isDynamic;

            Vector3.Cross(ref wheel.normal, ref wheel.slidingFriction.slidingFrictionAxis, out forceAxis);
            forceAxis.Normalize();
            //Do not need to check for normalize safety because normal and sliding friction axis must be perpendicular.

            linearAX = forceAxis.X;
            linearAY = forceAxis.Y;
            linearAZ = forceAxis.Z;

            //angular A = Ra x N
            angularAX = (wheel.ra.Y.Mul(linearAZ)).Sub((wheel.ra.Z.Mul(linearAY)));
            angularAY = (wheel.ra.Z.Mul(linearAX)).Sub((wheel.ra.X.Mul(linearAZ)));
            angularAZ = (wheel.ra.X.Mul(linearAY)).Sub((wheel.ra.Y.Mul(linearAX)));

            //Angular B = N x Rb
            angularBX = (linearAY.Mul(wheel.rb.Z)).Sub((linearAZ.Mul(wheel.rb.Y)));
            angularBY = (linearAZ.Mul(wheel.rb.X)).Sub((linearAX.Mul(wheel.rb.Z)));
            angularBZ = (linearAX.Mul(wheel.rb.Y)).Sub((linearAY.Mul(wheel.rb.X)));

            //Compute inverse effective mass matrix
            Fix32 entryA, entryB;

            //these are the transformed coordinates
            Fix32 tX, tY, tZ;
            if (vehicleEntity.isDynamic)
            {
                tX = ((angularAX.Mul(vehicleEntity.inertiaTensorInverse.M11)).Add(angularAY.Mul(vehicleEntity.inertiaTensorInverse.M21))).Add(angularAZ.Mul(vehicleEntity.inertiaTensorInverse.M31));
                tY = ((angularAX.Mul(vehicleEntity.inertiaTensorInverse.M12)).Add(angularAY.Mul(vehicleEntity.inertiaTensorInverse.M22))).Add(angularAZ.Mul(vehicleEntity.inertiaTensorInverse.M32));
                tZ = ((angularAX.Mul(vehicleEntity.inertiaTensorInverse.M13)).Add(angularAY.Mul(vehicleEntity.inertiaTensorInverse.M23))).Add(angularAZ.Mul(vehicleEntity.inertiaTensorInverse.M33));
                entryA = (((tX.Mul(angularAX)).Add(tY.Mul(angularAY))).Add(tZ.Mul(angularAZ))).Add(vehicleEntity.inverseMass);
            }
            else
                entryA = F64.C0;

            if (supportIsDynamic)
            {
                tX = ((angularBX.Mul(supportEntity.inertiaTensorInverse.M11)).Add(angularBY.Mul(supportEntity.inertiaTensorInverse.M21))).Add(angularBZ.Mul(supportEntity.inertiaTensorInverse.M31));
                tY = ((angularBX.Mul(supportEntity.inertiaTensorInverse.M12)).Add(angularBY.Mul(supportEntity.inertiaTensorInverse.M22))).Add(angularBZ.Mul(supportEntity.inertiaTensorInverse.M32));
                tZ = ((angularBX.Mul(supportEntity.inertiaTensorInverse.M13)).Add(angularBY.Mul(supportEntity.inertiaTensorInverse.M23))).Add(angularBZ.Mul(supportEntity.inertiaTensorInverse.M33));
                entryB = (((tX.Mul(angularBX)).Add(tY.Mul(angularBY))).Add(tZ.Mul(angularBZ))).Add(supportEntity.inverseMass);
            }
            else
                entryB = F64.C0;

            velocityToImpulse = (F64.C1.Neg()).Div((entryA.Add(entryB))); //Softness?

            currentFrictionCoefficient = gripFrictionBlender(gripFriction, wheel.supportMaterial.kineticFriction, true, wheel);

            //Compute the maximum force
            if (targetSpeed > F64.C0)
                maxMotorForceDt = maximumForwardForce.Mul(dt);
            else
                maxMotorForceDt = (maximumBackwardForce.Neg()).Mul(dt);




        }

        internal void ExclusiveUpdate()
        {
            //Warm starting
#if !WINDOWS
            Vector3 linear = new Vector3();
            Vector3 angular = new Vector3();
#else
            Vector3 linear, angular;
#endif
            linear.X = accumulatedImpulse.Mul(linearAX);
            linear.Y = accumulatedImpulse.Mul(linearAY);
            linear.Z = accumulatedImpulse.Mul(linearAZ);
            if (vehicleEntity.isDynamic)
            {
                angular.X = accumulatedImpulse.Mul(angularAX);
                angular.Y = accumulatedImpulse.Mul(angularAY);
                angular.Z = accumulatedImpulse.Mul(angularAZ);
                vehicleEntity.ApplyLinearImpulse(ref linear);
                vehicleEntity.ApplyAngularImpulse(ref angular);
            }
            if (supportIsDynamic)
            {
                linear.X = linear.X.Neg();
                linear.Y = linear.Y.Neg();
                linear.Z = linear.Z.Neg();
                angular.X = accumulatedImpulse.Mul(angularBX);
                angular.Y = accumulatedImpulse.Mul(angularBY);
                angular.Z = accumulatedImpulse.Mul(angularBZ);
                supportEntity.ApplyLinearImpulse(ref linear);
                supportEntity.ApplyAngularImpulse(ref angular);
            }
        }
    }
}