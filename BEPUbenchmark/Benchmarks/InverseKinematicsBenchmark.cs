﻿using BEPUik;
using BEPUphysics.CollisionRuleManagement;
using BEPUphysics.Constraints.TwoEntity.Joints;
using BEPUphysics.Constraints.TwoEntity.Motors;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using BEPUutilities;

using System.Collections.Generic;

namespace BEPUbenchmark.Benchmarks
{
	public class InverseKinematicsBenchmark : Benchmark
	{
		private IKSolver solver;

		private struct BoneRelationship
		{
			public Bone Bone;
			public Entity Entity;
			public Quaternion LocalRotationBoneToEntity;

			public BoneRelationship(Bone bone, Entity entity)
			{
				Bone = bone;
				Entity = entity;
				LocalRotationBoneToEntity = Quaternion.Concatenate(entity.Orientation, Quaternion.Conjugate(bone.Orientation));
			}
		}

		private List<BoneRelationship> bones;
		private List<Control> controls;
		private List<IKJoint> joints;

		private DragControl dragControl;


		protected override void InitializeSpace()
		{
			solver = new IKSolver();
			bones = new List<BoneRelationship>();
			controls = new List<Control>();
			joints = new List<IKJoint>();

			dragControl = new DragControl();

			Box ground = new Box(new Vector3(0.ToFix(), 0.ToFix(), 0.ToFix()), 30.ToFix(), 1.ToFix(), 30.ToFix());
			Space.Add(ground);

			solver.ActiveSet.UseAutomass = true;
			solver.AutoscaleControlImpulses = true;
			solver.AutoscaleControlMaximumForce = Fix32.MaxValue;
			solver.TimeStepDuration = .1m.ToFix();
			solver.ControlIterationCount = 100;
			solver.FixerIterationCount = 10;
			solver.VelocitySubiterationCount = 3;

			BuildActionFigure(new Vector3(5.ToFix(), 6.ToFix(), (-8).ToFix()));
			BuildActionFigure(new Vector3(5.ToFix(), 6.ToFix(), (-3).ToFix()));
			BuildActionFigure(new Vector3(5.ToFix(), 6.ToFix(), 3.ToFix()));
			BuildActionFigure(new Vector3(5.ToFix(), 6.ToFix(), 8.ToFix()));

			dragControl.TargetBone = bones[0].Bone;
			controls.Add(dragControl);
			dragControl.LinearMotor.Offset = new Vector3(0.ToFix(), 0.ToFix(), 0.2m.ToFix());
		}

		protected override void Step()
		{
			solver.Solve(controls);

			foreach (var bone in bones)
			{
				bone.Entity.Position = bone.Bone.Position;
				bone.Entity.Orientation = Quaternion.Concatenate(bone.LocalRotationBoneToEntity, bone.Bone.Orientation);
				bone.Entity.AngularVelocity = new Vector3();
				bone.Entity.LinearVelocity = new Vector3();
			}
		}

		void BuildActionFigure(Vector3 position)
		{
			//Make a simple, poseable action figure, like the ActionFigureDemo.
			Entity body = new Box(position, 1.5m.ToFix(), 2.ToFix(), 1.ToFix(), 10.ToFix());
			Space.Add(body);

			Entity head = new Sphere(body.Position + new Vector3(0.ToFix(), 2.ToFix(), 0.ToFix()), .5m.ToFix(), 5.ToFix());
			Space.Add(head);

			//Connect the head to the body.
			var headBodyBallSocketAnchor = head.Position + new Vector3(0.ToFix(), (-.75m).ToFix(), 0.ToFix());
			Space.Add(new BallSocketJoint(body, head, headBodyBallSocketAnchor));
			//Angular motors can be used to simulate friction when their goal velocity is 0.
			var angularMotor = new AngularMotor(body, head);
			angularMotor.Settings.MaximumForce = 150.ToFix(); //The maximum force of 'friction' in this joint.
			Space.Add(angularMotor);

			//Make the first arm.
			var upperLeftArm = new Box(body.Position + new Vector3((-1.6m).ToFix(), .8m.ToFix(), 0.ToFix()), 1.ToFix(), .5m.ToFix(), .5m.ToFix(), 5.ToFix());
			Space.Add(upperLeftArm);

			var lowerLeftArm = new Box(upperLeftArm.Position + new Vector3((-1.4m).ToFix(), 0.ToFix(), 0.ToFix()), 1.ToFix(), .5m.ToFix(), .5m.ToFix(), 5.ToFix());
			Space.Add(lowerLeftArm);

			var leftHand = new Box(lowerLeftArm.Position + new Vector3((-.8m).ToFix(), 0.ToFix(), 0.ToFix()), 0.5m.ToFix(), 0.3m.ToFix(), 0.5m.ToFix(), 4.ToFix());
			Space.Add(leftHand);

			//Connect the body to the upper arm.
			var bodyUpperLeftArmBallSocketAnchor = upperLeftArm.Position + new Vector3(.7m.ToFix(), 0.ToFix(), 0.ToFix());
			Space.Add(new BallSocketJoint(body, upperLeftArm, bodyUpperLeftArmBallSocketAnchor));
			angularMotor = new AngularMotor(body, upperLeftArm);
			angularMotor.Settings.MaximumForce = 250.ToFix();
			Space.Add(angularMotor);

			//Connect the upper arm to the lower arm.
			var upperLeftArmLowerLeftArmBallSocketAnchor = upperLeftArm.Position + new Vector3((-.7m).ToFix(), 0.ToFix(), 0.ToFix());
			Space.Add(new BallSocketJoint(upperLeftArm, lowerLeftArm, upperLeftArmLowerLeftArmBallSocketAnchor));
			angularMotor = new AngularMotor(upperLeftArm, lowerLeftArm);
			angularMotor.Settings.MaximumForce = 150.ToFix();
			Space.Add(angularMotor);

			//Connect the lower arm to the hand.
			var lowerLeftArmLeftHandBallSocketAnchor = lowerLeftArm.Position + new Vector3((-.5m).ToFix(), 0.ToFix(), 0.ToFix());
			Space.Add(new BallSocketJoint(lowerLeftArm, leftHand, lowerLeftArmLeftHandBallSocketAnchor));
			angularMotor = new AngularMotor(lowerLeftArm, leftHand);
			angularMotor.Settings.MaximumForce = 150.ToFix();
			Space.Add(angularMotor);

			//Make the second arm.
			var upperRightArm = new Box(body.Position + new Vector3(1.6m.ToFix(), .8m.ToFix(), 0.ToFix()), 1.ToFix(), .5m.ToFix(), .5m.ToFix(), 5.ToFix());
			Space.Add(upperRightArm);

			var lowerRightArm = new Box(upperRightArm.Position + new Vector3(1.4m.ToFix(), 0.ToFix(), 0.ToFix()), 1.ToFix(), .5m.ToFix(), .5m.ToFix(), 5.ToFix());
			Space.Add(lowerRightArm);

			var rightHand = new Box(lowerRightArm.Position + new Vector3(.8m.ToFix(), 0.ToFix(), 0.ToFix()), 0.5m.ToFix(), 0.3m.ToFix(), 0.5m.ToFix(), 4.ToFix());
			Space.Add(rightHand);

			//Connect the body to the upper arm.
			var bodyUpperRightArmBallSocketAnchor = upperRightArm.Position + new Vector3((-.7m).ToFix(), 0.ToFix(), 0.ToFix());
			Space.Add(new BallSocketJoint(body, upperRightArm, bodyUpperRightArmBallSocketAnchor));
			//Angular motors can be used to simulate friction when their goal velocity is 0.
			angularMotor = new AngularMotor(body, upperRightArm);
			angularMotor.Settings.MaximumForce = 250.ToFix(); //The maximum force of 'friction' in this joint.
			Space.Add(angularMotor);

			//Connect the upper arm to the lower arm.
			var upperRightArmLowerRightArmBallSocketAnchor = upperRightArm.Position + new Vector3(.7m.ToFix(), 0.ToFix(), 0.ToFix());
			Space.Add(new BallSocketJoint(upperRightArm, lowerRightArm, upperRightArmLowerRightArmBallSocketAnchor));
			angularMotor = new AngularMotor(upperRightArm, lowerRightArm);
			angularMotor.Settings.MaximumForce = 150.ToFix();
			Space.Add(angularMotor);

			//Connect the lower arm to the hand.
			var lowerRightArmRightHandBallSocketAnchor = lowerRightArm.Position + new Vector3(.5m.ToFix(), 0.ToFix(), 0.ToFix());
			Space.Add(new BallSocketJoint(lowerRightArm, rightHand, lowerRightArmRightHandBallSocketAnchor));
			angularMotor = new AngularMotor(lowerRightArm, rightHand);
			angularMotor.Settings.MaximumForce = 150.ToFix();
			Space.Add(angularMotor);

			//Make the first leg.
			var upperLeftLeg = new Box(body.Position + new Vector3((-.6m).ToFix(), (-2.1m).ToFix(), 0.ToFix()), .5m.ToFix(), 1.3m.ToFix(), .5m.ToFix(), 8.ToFix());
			Space.Add(upperLeftLeg);

			var lowerLeftLeg = new Box(upperLeftLeg.Position + new Vector3(0.ToFix(), (-1.7m).ToFix(), 0.ToFix()), .5m.ToFix(), 1.3m.ToFix(), .5m.ToFix(), 8.ToFix());
			Space.Add(lowerLeftLeg);

			var leftFoot = new Box(lowerLeftLeg.Position + new Vector3(0.ToFix(), (-.25m - 0.65m).ToFix(), 0.25m.ToFix()), .5m.ToFix(), .4m.ToFix(), 1.ToFix(), 8.ToFix());
			Space.Add(leftFoot);

			//Connect the body to the upper leg.
			var bodyUpperLeftLegBallSocketAnchor = upperLeftLeg.Position + new Vector3(0.ToFix(), .9m.ToFix(), 0.ToFix());
			Space.Add(new BallSocketJoint(body, upperLeftLeg, bodyUpperLeftLegBallSocketAnchor));
			//Angular motors can be used to simulate friction when their goal velocity is 0.
			angularMotor = new AngularMotor(body, upperLeftLeg);
			angularMotor.Settings.MaximumForce = 350.ToFix(); //The maximum force of 'friction' in this joint.
			Space.Add(angularMotor);

			//Connect the upper leg to the lower leg.
			var upperLeftLegLowerLeftLegBallSocketAnchor = upperLeftLeg.Position + new Vector3(0.ToFix(), (-.9m).ToFix(), 0.ToFix());
			Space.Add(new BallSocketJoint(upperLeftLeg, lowerLeftLeg, upperLeftLegLowerLeftLegBallSocketAnchor));
			angularMotor = new AngularMotor(upperLeftLeg, lowerLeftLeg);
			angularMotor.Settings.MaximumForce = 250.ToFix();
			Space.Add(angularMotor);

			//Connect the lower leg to the foot.
			var lowerLeftLegLeftFootBallSocketAnchor = lowerLeftLeg.Position + new Vector3(0.ToFix(), (-.65m).ToFix(), 0.ToFix());
			Space.Add(new BallSocketJoint(lowerLeftLeg, leftFoot, lowerLeftLegLeftFootBallSocketAnchor));
			angularMotor = new AngularMotor(lowerLeftLeg, leftFoot);
			angularMotor.Settings.MaximumForce = 250.ToFix();
			Space.Add(angularMotor);

			//Make the second leg.
			var upperRightLeg = new Box(body.Position + new Vector3(.6m.ToFix(), (-2.1m).ToFix(), 0.ToFix()), .5m.ToFix(), 1.3m.ToFix(), .5m.ToFix(), 8.ToFix());
			Space.Add(upperRightLeg);

			var lowerRightLeg = new Box(upperRightLeg.Position + new Vector3(0.ToFix(), (-1.7m).ToFix(), 0.ToFix()), .5m.ToFix(), 1.3m.ToFix(), .5m.ToFix(), 8.ToFix());
			Space.Add(lowerRightLeg);

			var rightFoot = new Box(lowerRightLeg.Position + new Vector3(0.ToFix(), (-.25m - 0.65m).ToFix(), 0.25m.ToFix()), .5m.ToFix(), .4m.ToFix(), 1.ToFix(), 8.ToFix());
			Space.Add(rightFoot);

			//Connect the body to the upper leg.
			var bodyUpperRightLegBallSocketAnchor = upperRightLeg.Position + new Vector3(0.ToFix(), .9m.ToFix(), 0.ToFix());
			Space.Add(new BallSocketJoint(body, upperRightLeg, bodyUpperRightLegBallSocketAnchor));
			//Angular motors can be used to simulate friction when their goal velocity is 0.
			angularMotor = new AngularMotor(body, upperRightLeg);
			angularMotor.Settings.MaximumForce = 350.ToFix(); //The maximum force of 'friction' in this joint.
			Space.Add(angularMotor);

			//Connect the upper leg to the lower leg.
			var upperRightLegLowerRightLegBallSocketAnchor = upperRightLeg.Position + new Vector3(0.ToFix(), (-.9m).ToFix(), 0.ToFix());
			Space.Add(new BallSocketJoint(upperRightLeg, lowerRightLeg, upperRightLegLowerRightLegBallSocketAnchor));
			angularMotor = new AngularMotor(upperRightLeg, lowerRightLeg);
			angularMotor.Settings.MaximumForce = 250.ToFix();
			Space.Add(angularMotor);

			//Connect the lower leg to the foot.
			var lowerRightLegRightFootBallSocketAnchor = lowerRightLeg.Position + new Vector3(0.ToFix(), (-.65m).ToFix(), 0.ToFix());
			Space.Add(new BallSocketJoint(lowerRightLeg, rightFoot, lowerRightLegRightFootBallSocketAnchor));
			angularMotor = new AngularMotor(lowerRightLeg, rightFoot);
			angularMotor.Settings.MaximumForce = 250.ToFix();
			Space.Add(angularMotor);

			//Set up collision rules.
			CollisionRules.AddRule(head, body, CollisionRule.NoBroadPhase);
			CollisionRules.AddRule(body, upperLeftArm, CollisionRule.NoBroadPhase);
			CollisionRules.AddRule(upperLeftArm, lowerLeftArm, CollisionRule.NoBroadPhase);
			CollisionRules.AddRule(lowerLeftArm, leftHand, CollisionRule.NoBroadPhase);
			CollisionRules.AddRule(body, upperRightArm, CollisionRule.NoBroadPhase);
			CollisionRules.AddRule(upperRightArm, lowerRightArm, CollisionRule.NoBroadPhase);
			CollisionRules.AddRule(lowerRightArm, rightHand, CollisionRule.NoBroadPhase);
			CollisionRules.AddRule(body, upperLeftLeg, CollisionRule.NoBroadPhase);
			CollisionRules.AddRule(upperLeftLeg, lowerLeftLeg, CollisionRule.NoBroadPhase);
			CollisionRules.AddRule(lowerLeftLeg, leftFoot, CollisionRule.NoBroadPhase);
			CollisionRules.AddRule(body, upperRightLeg, CollisionRule.NoBroadPhase);
			CollisionRules.AddRule(upperRightLeg, lowerRightLeg, CollisionRule.NoBroadPhase);
			CollisionRules.AddRule(lowerRightLeg, rightFoot, CollisionRule.NoBroadPhase);



			//IK version!
			Bone bodyBone = new Bone(body.Position, Quaternion.Identity, .75m.ToFix(), 2.ToFix());
			Bone headBone = new Bone(head.Position, Quaternion.Identity, .4m.ToFix(), .8m.ToFix());
			Bone upperLeftArmBone = new Bone(upperLeftArm.Position, Quaternion.CreateFromAxisAngle(  new Vector3(0.ToFix(), 0.ToFix(), 1.ToFix()), MathHelper.PiOver2), .25m.ToFix(), 1.ToFix());
			Bone lowerLeftArmBone = new Bone(lowerLeftArm.Position, Quaternion.CreateFromAxisAngle(  new Vector3(0.ToFix(), 0.ToFix(), 1.ToFix()), MathHelper.PiOver2), .25m.ToFix(), 1.ToFix());
			Bone upperRightArmBone = new Bone(upperRightArm.Position, Quaternion.CreateFromAxisAngle(new Vector3(0.ToFix(), 0.ToFix(), 1.ToFix()), MathHelper.PiOver2), .25m.ToFix(), 1.ToFix());
			Bone lowerRightArmBone = new Bone(lowerRightArm.Position, Quaternion.CreateFromAxisAngle(new Vector3(0.ToFix(), 0.ToFix(), 1.ToFix()), MathHelper.PiOver2), .25m.ToFix(), 1.ToFix());
			Bone leftHandBone = new Bone(leftHand.Position, Quaternion.CreateFromAxisAngle(new Vector3(0.ToFix(), 0.ToFix(), 1.ToFix()), MathHelper.PiOver2), .2m.ToFix(), .5m.ToFix());
			Bone rightHandBone = new Bone(rightHand.Position, Quaternion.CreateFromAxisAngle(new Vector3(0.ToFix(), 0.ToFix(), 1.ToFix()), MathHelper.PiOver2), .2m.ToFix(), .5m.ToFix());
			Bone upperLeftLegBone = new Bone(upperLeftLeg.Position, Quaternion.Identity, .25m.ToFix(), 1.3m.ToFix());
			Bone lowerLeftLegBone = new Bone(lowerLeftLeg.Position, Quaternion.Identity, .25m.ToFix(), 1.3m.ToFix());
			Bone leftFootBone = new Bone(leftFoot.Position, Quaternion.CreateFromAxisAngle(new Vector3(1.ToFix(), 0.ToFix(), 0.ToFix()), MathHelper.PiOver2), .25m.ToFix(), 1.ToFix());
			Bone upperRightLegBone = new Bone(upperRightLeg.Position, Quaternion.Identity, .25m.ToFix(), 1.3m.ToFix());
			Bone lowerRightLegBone = new Bone(lowerRightLeg.Position, Quaternion.Identity, .25m.ToFix(), 1.3m.ToFix());
			Bone rightFootBone = new Bone(rightFoot.Position, Quaternion.CreateFromAxisAngle(new Vector3(1.ToFix(), 0.ToFix(), 0.ToFix()), MathHelper.PiOver2), .25m.ToFix(), 1.ToFix());

			bones.Add(new BoneRelationship(bodyBone, body));
			bones.Add(new BoneRelationship(headBone, head));
			bones.Add(new BoneRelationship(upperLeftArmBone, upperLeftArm));
			bones.Add(new BoneRelationship(lowerLeftArmBone, lowerLeftArm));
			bones.Add(new BoneRelationship(upperRightArmBone, upperRightArm));
			bones.Add(new BoneRelationship(lowerRightArmBone, lowerRightArm));
			bones.Add(new BoneRelationship(leftHandBone, leftHand));
			bones.Add(new BoneRelationship(rightHandBone, rightHand));
			bones.Add(new BoneRelationship(upperLeftLegBone, upperLeftLeg));
			bones.Add(new BoneRelationship(lowerLeftLegBone, lowerLeftLeg));
			bones.Add(new BoneRelationship(leftFootBone, leftFoot));
			bones.Add(new BoneRelationship(upperRightLegBone, upperRightLeg));
			bones.Add(new BoneRelationship(lowerRightLegBone, lowerRightLeg));
			bones.Add(new BoneRelationship(rightFootBone, rightFoot));

			//[We don't care about the return values here. A bit weird, but the constructor puts the reference where it needs to go.]
			joints.Add(new IKBallSocketJoint(bodyBone, headBone, headBodyBallSocketAnchor));
			joints.Add(new IKSwingLimit(bodyBone, headBone, Vector3.Up, Vector3.Up, MathHelper.PiOver2));
			joints.Add(new IKTwistLimit(bodyBone, headBone, Vector3.Up, Vector3.Up, MathHelper.PiOver2));

			//Left arm
			joints.Add(new IKBallSocketJoint(bodyBone, upperLeftArmBone, bodyUpperLeftArmBallSocketAnchor));
			joints.Add(new IKSwingLimit(bodyBone, upperLeftArmBone, Vector3.Normalize(new Vector3((-1).ToFix(), 0.ToFix(), .3m.ToFix())), Vector3.Left, MathHelper.Pi.Mul(.65m.ToFix())));
			joints.Add(new IKTwistLimit(bodyBone, upperLeftArmBone, Vector3.Left, Vector3.Left, MathHelper.PiOver2) { Rigidity = 0.08m.ToFix() });
			joints.Add(new IKBallSocketJoint(upperLeftArmBone, lowerLeftArmBone, upperLeftArmLowerLeftArmBallSocketAnchor));
			joints.Add(new IKSwivelHingeJoint(upperLeftArmBone, lowerLeftArmBone, Vector3.Up, Vector3.Left));
			joints.Add(new IKSwingLimit(upperLeftArmBone, lowerLeftArmBone, Vector3.Normalize(new Vector3((-0.23m).ToFix(), 0.ToFix(), .97m.ToFix())), Vector3.Left, MathHelper.Pi.Mul(0.435m.ToFix())));
			joints.Add(new IKTwistLimit(upperLeftArmBone, lowerLeftArmBone, Vector3.Left, Vector3.Left, MathHelper.PiOver4) { Rigidity = 0.08m.ToFix() });
			joints.Add(new IKBallSocketJoint(lowerLeftArmBone, leftHandBone, lowerLeftArmLeftHandBallSocketAnchor));
			joints.Add(new IKSwingLimit(lowerLeftArmBone, leftHandBone, Vector3.Left, Vector3.Left, MathHelper.PiOver2));
			joints.Add(new IKTwistLimit(lowerLeftArmBone, leftHandBone, Vector3.Left, Vector3.Left, MathHelper.PiOver4) { Rigidity = 0.08m.ToFix() });

			//Right arm
			joints.Add(new IKBallSocketJoint(bodyBone, upperRightArmBone, bodyUpperRightArmBallSocketAnchor));
			joints.Add(new IKSwingLimit(bodyBone, upperRightArmBone, Vector3.Normalize(new Vector3(1.ToFix(), 0.ToFix(), .3m.ToFix())), Vector3.Right, MathHelper.Pi.Mul(.65m.ToFix())));
			joints.Add(new IKTwistLimit(bodyBone, upperRightArmBone, Vector3.Right, Vector3.Right, MathHelper.PiOver2) { Rigidity = 0.08m.ToFix() });
			joints.Add(new IKBallSocketJoint(upperRightArmBone, lowerRightArmBone, upperRightArmLowerRightArmBallSocketAnchor));
			joints.Add(new IKSwivelHingeJoint(upperRightArmBone, lowerRightArmBone, Vector3.Up, Vector3.Right));
			joints.Add(new IKSwingLimit(upperRightArmBone, lowerRightArmBone, Vector3.Normalize(new Vector3(0.23m.ToFix(), 0.ToFix(), .97m.ToFix())), Vector3.Right, MathHelper.Pi.Mul(0.435m.ToFix())));
			joints.Add(new IKTwistLimit(upperRightArmBone, lowerRightArmBone, Vector3.Right, Vector3.Right, MathHelper.PiOver4) { Rigidity = 0.08m.ToFix() });
			joints.Add(new IKBallSocketJoint(lowerRightArmBone, rightHandBone, lowerRightArmRightHandBallSocketAnchor));
			joints.Add(new IKSwingLimit(lowerRightArmBone, rightHandBone, Vector3.Right, Vector3.Right, MathHelper.PiOver2));
			joints.Add(new IKTwistLimit(lowerRightArmBone, rightHandBone, Vector3.Right, Vector3.Right, MathHelper.PiOver4) { Rigidity = 0.08m.ToFix() });

			//Left Leg
			joints.Add(new IKBallSocketJoint(bodyBone, upperLeftLegBone, bodyUpperLeftLegBallSocketAnchor));
			joints.Add(new IKSwingLimit(bodyBone, upperLeftLegBone, Vector3.Normalize(new Vector3((-.3m).ToFix(), (-1).ToFix(), .6m.ToFix())), Vector3.Down, MathHelper.Pi.Mul(0.6m.ToFix())));
			joints.Add(new IKTwistLimit(bodyBone, upperLeftLegBone, Vector3.Up, Vector3.Up, MathHelper.PiOver4) { MeasurementAxisA = Vector3.Normalize(new Vector3((-1).ToFix(), 0.ToFix(), 1.ToFix())), Rigidity = 0.08m.ToFix() });
			joints.Add(new IKBallSocketJoint(upperLeftLegBone, lowerLeftLegBone, upperLeftLegLowerLeftLegBallSocketAnchor));
			joints.Add(new IKSwivelHingeJoint(upperLeftLegBone, lowerLeftLegBone, Vector3.Left, Vector3.Down));
			joints.Add(new IKTwistLimit(upperLeftLegBone, lowerLeftLegBone, Vector3.Up, Vector3.Up, MathHelper.Pi.Mul(.1m.ToFix())) { Rigidity = 0.08m.ToFix() });
			joints.Add(new IKSwingLimit(upperLeftLegBone, lowerLeftLegBone, Vector3.Normalize(new Vector3(0.ToFix(), (-.23m).ToFix(), (-.97m).ToFix())), Vector3.Down, MathHelper.Pi.Mul(0.435m.ToFix())));
			joints.Add(new IKBallSocketJoint(lowerLeftLegBone, leftFootBone, lowerLeftLegLeftFootBallSocketAnchor));
			joints.Add(new IKTwistJoint(lowerLeftLegBone, leftFootBone, Vector3.Down, Vector3.Down) { Rigidity = 0.08m.ToFix() });
			joints.Add(new IKSwingLimit(lowerLeftLegBone, leftFootBone, Vector3.Normalize(new Vector3(0.ToFix(), (-1).ToFix(), (-.3m).ToFix())), Vector3.Down, MathHelper.Pi.Mul(0.22m.ToFix())));

			//Right leg
			joints.Add(new IKBallSocketJoint(bodyBone, upperRightLegBone, bodyUpperRightLegBallSocketAnchor));
			joints.Add(new IKSwingLimit(bodyBone, upperRightLegBone, Vector3.Normalize(new Vector3(.3m.ToFix(), (-1).ToFix(), .6m.ToFix())), Vector3.Down, MathHelper.Pi.Mul(0.6m.ToFix())));
			joints.Add(new IKTwistLimit(bodyBone, upperRightLegBone, Vector3.Up, Vector3.Up, MathHelper.PiOver4) { MeasurementAxisA = Vector3.Normalize(new Vector3(1.ToFix(), 0.ToFix(), 1.ToFix())), Rigidity = 0.08m.ToFix() });
			joints.Add(new IKBallSocketJoint(upperRightLegBone, lowerRightLegBone, upperRightLegLowerRightLegBallSocketAnchor));
			joints.Add(new IKSwivelHingeJoint(upperRightLegBone, lowerRightLegBone, Vector3.Right, Vector3.Down));
			joints.Add(new IKTwistLimit(upperRightLegBone, lowerRightLegBone, Vector3.Up, Vector3.Up, MathHelper.Pi.Mul(.1m.ToFix())) { Rigidity = 0.08m.ToFix() });
			joints.Add(new IKSwingLimit(upperRightLegBone, lowerRightLegBone, Vector3.Normalize(new Vector3(0.ToFix(), (-.23m).ToFix(), (-.97m).ToFix())), Vector3.Down, MathHelper.Pi.Mul(0.435m.ToFix())));
			joints.Add(new IKBallSocketJoint(lowerRightLegBone, rightFootBone, lowerRightLegRightFootBallSocketAnchor));
			joints.Add(new IKTwistJoint(lowerRightLegBone, rightFootBone, Vector3.Down, Vector3.Down) { Rigidity = 0.08m.ToFix() });
			joints.Add(new IKSwingLimit(lowerRightLegBone, rightFootBone, Vector3.Normalize(new Vector3(0.ToFix(), (-1).ToFix(), (-.3m).ToFix())), Vector3.Down, MathHelper.Pi.Mul(0.22m.ToFix())));
		}
	}
}
