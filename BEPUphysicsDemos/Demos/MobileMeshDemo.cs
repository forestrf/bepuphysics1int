﻿using BEPUphysics.Entities.Prefabs;
using BEPUutilities;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics.CollisionShapes;


namespace BEPUphysicsDemos.Demos
{
    /// <summary>
    /// Demo showing a set of entity meshes capable of dynamic movement.
    /// </summary>
    public class MobileMeshDemo : StandardDemo
    {
        /// <summary>
        /// Constructs a new demo.
        /// </summary>
        /// <param name="game">Game owning this demo.</param>
        public MobileMeshDemo(DemosGame game)
            : base(game)
        {

            Vector3[] vertices;
            int[] indices;

            //Create a big hollow sphere (squished into an ellipsoid).
            ModelDataExtractor.GetVerticesAndIndicesFromModel(game.Content.Load<Model>("hollowsphere"), out vertices, out indices);
            var transform = new AffineTransform(new Vector3(.06m.ToFix(), .04m.ToFix(), .06m.ToFix()), Quaternion.Identity, new Vector3(0.ToFix(), 0.ToFix(), 0.ToFix()));

            //Note that meshes can also be made solid (MobileMeshSolidity.Solid).  This gives meshes a solid collidable volume, instead of just
            //being thin shells.  However, enabling solidity is more expensive.
            var mesh = new MobileMesh(vertices, indices, transform, MobileMeshSolidity.Counterclockwise);
            mesh.Position = new Vector3(0.ToFix(), 0.ToFix(), 0.ToFix());
            //Make the mesh spin a bit!
            mesh.AngularVelocity = new Vector3(0.ToFix(), 1.ToFix(), 0.ToFix());
            Space.Add(mesh);

            //Add another mobile mesh inside.
            ModelDataExtractor.GetVerticesAndIndicesFromModel(game.Content.Load<Model>("tube"), out vertices, out indices);
            transform = new AffineTransform(new Vector3(1.ToFix(), 1.ToFix(), 1.ToFix()), Quaternion.Identity, new Vector3(0.ToFix(), 0.ToFix(), 0.ToFix()));
            mesh = new MobileMesh(vertices, indices, transform, MobileMeshSolidity.Counterclockwise, 10.ToFix());
            mesh.Position = new Vector3(0.ToFix(), 10.ToFix(), 0.ToFix());
            Space.Add(mesh);

            //Create a bunch of boxes.
#if WINDOWS
            int numColumns = 5;
            int numRows = 5;
            int numHigh = 5;
#else
            //Keep the simulation a bit smaller on the xbox.
            int numColumns = 4;
            int numRows = 4;
            int numHigh = 4;
#endif
            Fix32 separation = 1.5m.ToFix();


            for (int i = 0; i < numRows; i++)
                for (int j = 0; j < numColumns; j++)
                    for (int k = 0; k < numHigh; k++)
                    {
                        Space.Add(new Box(new Vector3(separation.Mul(i.ToFix()), k.ToFix().Mul(separation), separation.Mul(j.ToFix())), 1.ToFix(), 1.ToFix(), 1.ToFix(), 5.ToFix()));
                    }

            //Space.Add(new Box(new Vector3(0, -10, 0), 1, 1, 1));
            game.Camera.Position = new Vector3(0.ToFix(), (-10).ToFix(), 5.ToFix());


        }


        public override void Update(Fix32 dt)
        {
            base.Update(dt);
        }


        /// <summary>
        /// Gets the name of the simulation.
        /// </summary>
        public override string Name
        {
            get { return "MobileMeshes"; }
        }
    }
}