﻿using System;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.Entities.Prefabs;
using BEPUutilities;
using FixMath.NET;

namespace BEPUphysicsDemos.Demos
{
    /// <summary>
    /// Boxes fall onto a large terrain.  Try driving around on it!
    /// </summary>
    public class TerrainDemo : StandardDemo
    {
        /// <summary>
        /// Constructs a new demo.
        /// </summary>
        /// <param name="game">Game owning this demo.</param>
        public TerrainDemo(DemosGame game)
            : base(game)
        {
            //x and y, in terms of heightmaps, refer to their local x and y coordinates.  In world space, they correspond to x and z.
            //Setup the heights of the terrain.
            //[The size here is limited by the Reach profile the demos use- the drawer draws the terrain as a big block and runs into primitive drawing limits.
            //The physics can support far larger terrains!]
            int xLength = 180;
            int zLength = 180;

            Fix64 xSpacing = 8.ToFix();
            Fix64 zSpacing = 8.ToFix();
            var heights = new Fix64[xLength, zLength];
            for (int i = 0; i < xLength; i++)
            {
                for (int j = 0; j < zLength; j++)
                {
                    Fix64 x = (i - xLength / 2).ToFix();
                    Fix64 z = (j - zLength / 2).ToFix();
                    //heights[i,j] = (Fix64)(x * y / 1000f);
                    heights[i, j] = 10.ToFix().Mul((Fix64.Sin(x.Div(8.ToFix())).Add(Fix64.Sin(z.Div(8.ToFix())))));
                    //heights[i,j] = 3 * (Fix64)Math.Sin(x * y / 100f);
                    //heights[i,j] = (x * x * x * y - y * y * y * x) / 1000f;
                }
            }
            //Create the terrain.
            var terrain = new Terrain(heights, new AffineTransform(
                    new Vector3(xSpacing, 1.ToFix(), zSpacing),
                    Quaternion.Identity,
                    new Vector3((xLength.ToFix().Neg().Mul(xSpacing)).Div(2.ToFix()), 0.ToFix(), (zLength.ToFix().Neg().Mul(zSpacing)).Div(2.ToFix()))));
            terrain.Shape.QuadTriangleOrganization = BEPUphysics.CollisionShapes.QuadTriangleOrganization.BottomRightUpperLeft;

            //terrain.Thickness = 5; //Uncomment this and shoot some things at the bottom of the terrain! They'll be sucked up through the ground.


            Space.Add(terrain);
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    for (int k = 0; k < 5; k++)
                    {
                        Space.Add(new Box(
                            new Vector3((0 + i * 4).ToFix(), (100 - j * 10).ToFix(), (0 + k * 4).ToFix()),
                            (2 + i * j * k).ToFix(),
                            (2 + i * j * k).ToFix(),
                            (2 + i * j * k).ToFix(),
                            (4 + 20 * i * j * k).ToFix()));
                    }
                }
            }



            game.ModelDrawer.Add(terrain);

            game.Camera.Position = new Vector3(0.ToFix(), 30.ToFix(), 20.ToFix());

        }





        /// <summary>
        /// Gets the name of the simulation.
        /// </summary>
        public override string Name
        {
            get { return "Terrain"; }
        }
    }
}