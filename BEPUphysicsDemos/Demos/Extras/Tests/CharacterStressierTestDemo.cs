﻿using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.Character;
using BEPUphysics.CollisionShapes;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUphysics.Entities.Prefabs;
using BEPUutilities;
using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework.Graphics;


namespace BEPUphysicsDemos.Demos.Extras.Tests
{
    /// <summary>
    /// A nice landscape full of stranger people.
    /// </summary>
    public class CharacterStressierTestDemo : StandardDemo
    {
        /// <summary>
        /// Constructs a new demo.
        /// </summary>
        /// <param name="game">Game owning this demo.</param>
        public CharacterStressierTestDemo(DemosGame game)
            : base(game)
        {
            //Load in mesh data and create the group.
            Vector3[] staticTriangleVertices;
            int[] staticTriangleIndices;

            var playgroundModel = game.Content.Load<Model>("playground");
            //This is a little convenience method used to extract vertices and indices from a model.
            //It doesn't do anything special; any approach that gets valid vertices and indices will work.
            ModelDataExtractor.GetVerticesAndIndicesFromModel(playgroundModel, out staticTriangleVertices, out staticTriangleIndices);
            var meshShape = new InstancedMeshShape(staticTriangleVertices, staticTriangleIndices);
            var meshes = new List<Collidable>();

            var xSpacing = 400;
            var ySpacing = 400;
            var xCount = 11;
            var yCount = 11;
            for (int i = 0; i < xCount; i++)
            {
                for (int j = 0; j < yCount; j++)
                {
                    var staticMesh = new InstancedMesh(meshShape, new AffineTransform(Matrix3x3.Identity, new Vector3((-xSpacing * (xCount - 1) / 2 + i * xSpacing).ToFix(), 0.ToFix(), (-ySpacing * (yCount - 1) / 2 + j * ySpacing).ToFix())));
                    staticMesh.Sidedness = TriangleSidedness.Counterclockwise;
                    Space.Add(staticMesh);
                    //meshes.Add(staticMesh);
                    game.ModelDrawer.Add(staticMesh);
                }
            }
            //var group = new StaticGroup(meshes);
            //Space.Add(group);

            //To demonstrate, we'll be creating a set of static objects and giving them to a group to manage.
            var collidables = new List<Collidable>();

            //Start with a whole bunch of boxes.  These are entity collidables, but without entities!
            xSpacing = 25;
            ySpacing = 16;
            Fix zSpacing = 25.ToFix();

            xCount = 25;
            yCount = 7;
            int zCount = 25;


            var random = new Random();
            for (int i = 0; i < xCount; i++)
            {
                for (int j = 0; j < yCount; j++)
                {
                    for (int k = 0; k < zCount; k++)
                    {
                        //Create a transform and the instance of the mesh.
                        var collidable = new ConvexCollidable<BoxShape>(new BoxShape((random.NextDouble().ToFix().Mul(25.ToFix())).Add(5.5m.ToFix()), (random.NextDouble().ToFix().Mul(25.ToFix())).Add(5.5m.ToFix()), random.NextDouble().ToFix().Mul(25.ToFix()).Add(5.5m.ToFix())));

                        //This EntityCollidable isn't associated with an entity, so we must manually tell it where to sit by setting the WorldTransform.
                        //This also updates its bounding box.
                        collidable.WorldTransform = new RigidTransform(
                            new Vector3((i * xSpacing - xCount * xSpacing * .5m).ToFix(), (j * ySpacing + -50).ToFix(), (k.ToFix().Mul(zSpacing)).Sub((zCount.ToFix().Mul(zSpacing)).Mul(.5m.ToFix()))),
                            Quaternion.CreateFromAxisAngle(Vector3.Normalize(new Vector3(random.NextDouble().ToFix(), random.NextDouble().ToFix(), random.NextDouble().ToFix())), random.NextDouble().ToFix().Mul(100.ToFix())));

                        collidables.Add(collidable);
                        game.ModelDrawer.Add(collidable);
                    }
                }
            }
            var group = new StaticGroup(collidables);
            Space.Add(group);

           
            //Now drop the characters on it!
            var numColumns = 16;
            var numRows = 16;
            var numHigh = 16;
            Fix separation = 24.ToFix();

            for (int i = 0; i < numRows; i++)
                for (int j = 0; j < numColumns; j++)
                    for (int k = 0; k < numHigh; k++)
                    {
                        var character = new CharacterController();
                        character.Body.Position =
                            new Vector3(
(separation.Mul(i.ToFix())).Sub((numRows.ToFix().Mul(separation)).Div(2.ToFix())),
50.ToFix().Add(k.ToFix().Mul(separation)),
(separation.Mul(j.ToFix())).Sub((numColumns.ToFix().Mul(separation)).Div(2.ToFix())));

                        characters.Add(character);

                        Space.Add(character);
                    }

           

            game.Camera.Position = new Vector3(0.ToFix(), 10.ToFix(), 40.ToFix());

            //Dump some boxes on top of the characters for fun.
            numColumns = 16;
            numRows = 16;
            numHigh = 8;
            separation = 24.ToFix();
            for (int i = 0; i < numRows; i++)
                for (int j = 0; j < numColumns; j++)
                    for (int k = 0; k < numHigh; k++)
                    {
                        var toAdd = new Box(
                            new Vector3(
(separation.Mul(i.ToFix())).Sub((numRows.ToFix().Mul(separation)).Div(2.ToFix())),
52.ToFix().Add(k.ToFix().Mul(separation)),
(separation.Mul(j.ToFix())).Sub((numColumns.ToFix().Mul(separation)).Div(2.ToFix()))),
0.8m.ToFix(), 0.8m.ToFix(), 0.8m.ToFix(), 15.ToFix());
                        toAdd.PositionUpdateMode = BEPUphysics.PositionUpdating.PositionUpdateMode.Continuous;

                        Space.Add(toAdd);
                    }
        }


        List<CharacterController> characters = new List<CharacterController>();
        List<SphereCharacterController> sphereCharacters = new List<SphereCharacterController>();
        Random random = new Random();

        /// <summary>
        /// Gets the name of the simulation.
        /// </summary>
        public override string Name
        {
            get { return "Character Stressier Test"; }
        }

        public override void Update(Fix dt)
        {
            //Tell all the characters to run around randomly.
            for (int i = 0; i < characters.Count; i++)
            {
                characters[i].HorizontalMotionConstraint.MovementDirection = new Vector2((random.NextDouble() * 2 - 1).ToFix(), (random.NextDouble() * 2 - 1).ToFix());
                if (random.NextDouble() < .01f)
                    characters[i].Jump();

                var next = random.NextDouble();
                if (next < .01)
                {
                    //Note: The character's graphic won't represent the crouching process properly since we're not remove/readding it.
                    if (next < .005f && characters[i].StanceManager.CurrentStance == Stance.Standing)
                        characters[i].StanceManager.DesiredStance = Stance.Crouching;
                    else
                        characters[i].StanceManager.DesiredStance = Stance.Standing;
                }
            }

            //Tell the sphere characters to run around too.
            for (int i = 0; i < sphereCharacters.Count; i++)
            {
                sphereCharacters[i].HorizontalMotionConstraint.MovementDirection = new Vector2((random.NextDouble() * 2 - 1).ToFix(), (random.NextDouble() * 2 - 1).ToFix());
                if (random.NextDouble() < .01f)
                    sphereCharacters[i].Jump();
            }


            base.Update(dt);
        }

        //public override void DrawUI()
        //{
        //    //Try compiling the library with the PROFILE symbol defined and using this!
        //    Game.DataTextDrawer.Draw("Time Step Stage Times: ", new Vector2(20, 10));

        //    Game.TinyTextDrawer.Draw("SpaceObjectBuffer: ", Space.SpaceObjectBuffer.Time * 1000, 2, new Vector2(20, 35));
        //    Game.TinyTextDrawer.Draw("Entity State Write Buffer: ", Space.EntityStateWriteBuffer.Time * 1000, 2, new Vector2(20, 50));
        //    Game.TinyTextDrawer.Draw("Deactivation: ", Space.DeactivationManager.Time * 1000, 2, new Vector2(20, 65));
        //    Game.TinyTextDrawer.Draw("ForceUpdater: ", Space.ForceUpdater.Time * 1000, 2, new Vector2(20, 80));
        //    Game.TinyTextDrawer.Draw("DuringForcesUpdateables: ", Space.DuringForcesUpdateables.Time * 1000, 2, new Vector2(20, 95));
        //    Game.TinyTextDrawer.Draw("Bounding Boxes: ", Space.BoundingBoxUpdater.Time * 1000, 2, new Vector2(20, 110));
        //    Game.TinyTextDrawer.Draw("BroadPhase: ", Space.BroadPhase.Time * 1000, 2, new Vector2(20, 125));
        //    Game.TinyTextDrawer.Draw("     Refit: ", (Space.BroadPhase as DynamicHierarchy).RefitTime * 1000, 2, new Vector2(20, 140));
        //    Game.TinyTextDrawer.Draw("     Overlap: ", (Space.BroadPhase as DynamicHierarchy).OverlapTime * 1000, 2, new Vector2(20, 155));
        //    Game.TinyTextDrawer.Draw("BeforeNarrowPhaseUpdateables: ", Space.BeforeNarrowPhaseUpdateables.Time * 1000, 2, new Vector2(20, 170));
        //    Game.TinyTextDrawer.Draw("NarrowPhase: ", Space.NarrowPhase.Time * 1000, 2, new Vector2(20, 185));
        //    Game.TinyTextDrawer.Draw("     Pair Updates: ", Space.NarrowPhase.PairUpdateTime * 1000, 2, new Vector2(20, 200));
        //    Game.TinyTextDrawer.Draw("     Flush New: ", Space.NarrowPhase.FlushNewPairsTime * 1000, 2, new Vector2(20, 215));
        //    Game.TinyTextDrawer.Draw("     Flush Solver Updateables: ", Space.NarrowPhase.FlushSolverUpdateableChangesTime * 1000, 2, new Vector2(20, 230));
        //    Game.TinyTextDrawer.Draw("     Stale Removal: ", Space.NarrowPhase.StaleOverlapRemovalTime * 1000, 2, new Vector2(20, 245));
        //    Game.TinyTextDrawer.Draw("BeforeSolverUpdateables: ", Space.BeforeSolverUpdateables.Time * 1000, 2, new Vector2(20, 260));
        //    Game.TinyTextDrawer.Draw("Solver: ", Space.Solver.Time * 1000, 2, new Vector2(20, 275));
        //    Game.TinyTextDrawer.Draw("BeforePositionUpdateUpdateables: ", Space.BeforePositionUpdateUpdateables.Time * 1000, 2, new Vector2(20, 290));
        //    Game.TinyTextDrawer.Draw("Position Update: ", Space.PositionUpdater.Time * 1000, 2, new Vector2(20, 305));
        //    Game.TinyTextDrawer.Draw("Read Buffers States Update: ", Space.BufferedStates.ReadBuffers.Time * 1000, 2, new Vector2(20, 320));
        //    Game.TinyTextDrawer.Draw("Deferred Event Dispatcher: ", Space.DeferredEventDispatcher.Time * 1000, 2, new Vector2(20, 335));
        //    Game.TinyTextDrawer.Draw("EndOfTimeStepUpdateables: ", Space.EndOfTimeStepUpdateables.Time * 1000, 2, new Vector2(20, 350));


        //    Game.DataTextDrawer.Draw("Total: ", Space.Time * 1000, 2, new Vector2(20, 375));
        //    base.DrawUI();
        //}




    }
}