using System;
using BEPUphysics.Entities;
using BEPUphysics.UpdateableSystems.ForceFields;
using BEPUutilities;


namespace BEPUphysicsDemos.SampleCode
{
    /// <summary>
    /// Applies a force on all objects within a field proportional to the inverse square of the distance.
    /// </summary>
    public class GravitationalField : ForceField
    {
        /// <summary>
        /// Creates a gravitational field.
        /// </summary>
        /// <param name="shape">Shape representing the volume of the force field.</param>
        /// <param name="origin">Location that entities will be pushed toward.</param>
        /// <param name="multiplier">Represents the gravitational constant of the field times the effective mass at the center of the field.</param>
        /// <param name="maxAcceleration">Maximum acceleration the field can apply.</param>
        public GravitationalField(ForceFieldShape shape, Vector3 origin, Fix multiplier, Fix maxAcceleration)
            : base(shape)
        {
            this.Multiplier = multiplier;
            this.Origin = origin;
            this.MaxAcceleration = maxAcceleration;
        }

        /// <summary>
        /// Gets or sets the gravitational constant of the field times the effective mass at the center of the field.
        /// </summary>
        public Fix Multiplier { get; set; }

        /// <summary>
        /// Gets or sets the maximum acceleration that can be applied by the field.
        /// </summary>
        public Fix MaxAcceleration { get; set; }

        /// <summary>
        /// Gets or sets the center of the field that entities will be pushed toward.
        /// </summary>
        public Vector3 Origin { get; set; }


        /// <summary>
        /// Calculates the gravitational force to apply to the entity.
        /// </summary>
        /// <param name="e">Target of the impulse.</param>
        /// <param name="dt">Time since the last frame in simulation seconds.</param>
        /// <param name="impulse">Force to apply at the given position.</param>
        protected override void CalculateImpulse(Entity e, Fix dt, out Vector3 impulse)
        {
            Vector3 r = e.Position - Origin;
            Fix length = r.Length();
            if (length > Toolbox.BigEpsilon)
            {
                Fix force = (dt.Mul(e.Mass)).Mul(MathHelper.Min(MaxAcceleration, Multiplier.Div((length.Mul(length)))));
                impulse = (force.Div(length)).Neg() * r; //Extra division by length normalizes the direction.
            }
            else
                impulse = new Vector3();


            ////Could use a linear dropoff for a slightly faster calculation (divide by length^2 instead of length^3).
            //Vector3 r = e.Position - Origin;
            //Fix64 length = r.Length();
            //if (length > Toolbox.BigEpsilon)
            //{
            //Fix64 force = dt * e.Mass * Math.Min(MaxAcceleration, Multiplier / length);
            //impulse = -(force / length) * r; //Extra division by length normalizes the direction.
            //}
            //else
            //    impulse = new Vector3();
        }
    }
}