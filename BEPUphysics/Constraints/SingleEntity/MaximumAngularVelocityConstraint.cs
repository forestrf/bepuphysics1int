﻿using System;
using BEPUphysics.Entities;
using BEPUutilities;


namespace BEPUphysics.Constraints.SingleEntity
{
    /// <summary>
    /// Prevents the target entity from moving faster than the specified speeds.
    /// </summary>
    public class MaximumAngularSpeedConstraint : SingleEntityConstraint, I3DImpulseConstraint
    {
        private Matrix3x3 effectiveMassMatrix;
        private Fix maxForceDt = Fix.MaxValue;
        private Fix maxForceDtSquared = Fix.MaxValue;
        private Vector3 accumulatedImpulse;
        private Fix maximumForce = Fix.MaxValue;
        private Fix maximumSpeed;
        private Fix maximumSpeedSquared;

        private Fix softness = .00001m.ToFix();
        private Fix usedSoftness;

        /// <summary>
        /// Constructs a maximum speed constraint.
        /// Set its Entity and MaximumSpeed to complete the configuration.
        /// IsActive also starts as false with this constructor.
        /// </summary>
        public MaximumAngularSpeedConstraint()
        {
            IsActive = false;
        }

        /// <summary>
        /// Constructs a maximum speed constraint.
        /// </summary>
        /// <param name="e">Affected entity.</param>
        /// <param name="maxSpeed">Maximum angular speed allowed.</param>
        public MaximumAngularSpeedConstraint(Entity e, Fix maxSpeed)
        {
            Entity = e;
            MaximumSpeed = maxSpeed;
        }

        /// <summary>
        /// Gets and sets the maximum impulse that the constraint will attempt to apply when satisfying its requirements.
        /// This field can be used to simulate friction in a constraint.
        /// </summary>
        public Fix MaximumForce
        {
            get
            {
                if (maximumForce > F64.C0)
                {
                    return maximumForce;
                }
                return F64.C0;
            }
            set { maximumForce = value >= F64.C0 ? value : F64.C0; }
        }

        /// <summary>
        /// Gets or sets the maximum angular speed that this constraint allows.
        /// </summary>
        public Fix MaximumSpeed
        {
            get { return maximumSpeed; }
            set
            {
                maximumSpeed = MathHelper.Max(F64.C0, value);
                maximumSpeedSquared = maximumSpeed.Mul(maximumSpeed);
            }
        }


        /// <summary>
        /// Gets and sets the softness of this constraint.
        /// Higher values of softness allow the constraint to be violated more.
        /// Must be greater than zero.
        /// Sometimes, if a joint system is unstable, increasing the softness of the involved constraints will make it settle down.
        /// For motors, softness can be used to implement damping.  For a damping constant k, the appropriate softness is 1/k.
        /// </summary>
        public Fix Softness
        {
            get { return softness; }
            set { softness = MathHelper.Max(F64.C0, value); }
        }

        #region I3DImpulseConstraint Members

        /// <summary>
        /// Gets the current relative velocity between the connected entities with respect to the constraint.
        /// </summary>
        Vector3 I3DImpulseConstraint.RelativeVelocity
        {
            get { return entity.angularVelocity; }
        }

        /// <summary>
        /// Gets the total impulse applied by the constraint.
        /// </summary>
        public Vector3 TotalImpulse
        {
            get { return accumulatedImpulse; }
        }

        #endregion

        /// <summary>
        /// Calculates and applies corrective impulses.
        /// Called automatically by space.
        /// </summary>
        public override Fix SolveIteration()
        {
            Fix angularSpeed = entity.angularVelocity.LengthSquared();
            if (angularSpeed > maximumSpeedSquared)
            {
                angularSpeed = Fix32Ext.Sqrt(angularSpeed);
                Vector3 impulse;
                //divide by angularSpeed to normalize the velocity.
                //Multiply by angularSpeed - maximumSpeed to get the 'velocity change vector.'
                Vector3.Multiply(ref entity.angularVelocity, (angularSpeed.Sub(maximumSpeed).Neg()).Div(angularSpeed), out impulse);

                //incorporate softness
                Vector3 softnessImpulse;
                Vector3.Multiply(ref accumulatedImpulse, usedSoftness, out softnessImpulse);
                Vector3.Subtract(ref impulse, ref softnessImpulse, out impulse);

                //Transform into impulse
                Matrix3x3.Transform(ref impulse, ref effectiveMassMatrix, out impulse);


                //Accumulate
                Vector3 previousAccumulatedImpulse = accumulatedImpulse;
                Vector3.Add(ref accumulatedImpulse, ref impulse, out accumulatedImpulse);
                Fix forceMagnitude = accumulatedImpulse.LengthSquared();
                if (forceMagnitude > maxForceDtSquared)
                {
                    //max / impulse gives some value 0 < x < 1.  Basically, normalize the vector (divide by the length) and scale by the maximum.
                    Fix multiplier = maxForceDt.Div(Fix32Ext.Sqrt(forceMagnitude));
					accumulatedImpulse.X = accumulatedImpulse.X.Mul(multiplier);
					accumulatedImpulse.Y = accumulatedImpulse.Y.Mul(multiplier);
					accumulatedImpulse.Z = accumulatedImpulse.Z.Mul(multiplier);

                    //Since the limit was exceeded by this corrective impulse, limit it so that the accumulated impulse remains constrained.
                    impulse.X = accumulatedImpulse.X.Sub(previousAccumulatedImpulse.X);
                    impulse.Y = accumulatedImpulse.Y.Sub(previousAccumulatedImpulse.Y);
                    impulse.Z = accumulatedImpulse.Z.Sub(previousAccumulatedImpulse.Z);
                }

                entity.ApplyAngularImpulse(ref impulse);


                return ((Fix32Ext.Abs(impulse.X).Add(Fix32Ext.Abs(impulse.Y))).Add(Fix32Ext.Abs(impulse.Z)));
            }

            return F64.C0;
        }

        /// <summary>
        /// Calculates necessary information for velocity solving.
        /// Called automatically by space.
        /// </summary>
        /// <param name="dt">Time in seconds since the last update.</param>
        public override void Update(Fix dt)
        {
            usedSoftness = softness.Div(dt);

            effectiveMassMatrix = entity.inertiaTensorInverse;

			effectiveMassMatrix.M11 =
effectiveMassMatrix.M11.Add(usedSoftness);
			effectiveMassMatrix.M22 = effectiveMassMatrix.M22.Add(usedSoftness);
			effectiveMassMatrix.M33 = effectiveMassMatrix.M33.Add(usedSoftness);

            Matrix3x3.Invert(ref effectiveMassMatrix, out effectiveMassMatrix);

            //Determine maximum force
            if (maximumForce < Fix.MaxValue)
            {
                maxForceDt = maximumForce.Mul(dt);
                maxForceDtSquared = maxForceDt.Mul(maxForceDt);
            }
            else
            {
                maxForceDt = Fix.MaxValue;
                maxForceDtSquared = Fix.MaxValue;
            }

        }


        public override void ExclusiveUpdate()
        {

            //Can't do warmstarting due to the strangeness of this constraint (not based on a position error, nor is it really a motor).
            accumulatedImpulse = Toolbox.ZeroVector;
        }
    }
}