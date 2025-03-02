﻿using System;
using NitroxClient.GameLogic;
using NitroxClient.Unity.Smoothing;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public abstract class MultiplayerVehicleControl : MonoBehaviour
    {
        private Rigidbody rigidbody;

        protected readonly SmoothParameter SmoothYaw = new SmoothParameter();
        protected readonly SmoothParameter SmoothPitch = new SmoothParameter();
        protected readonly SmoothVector SmoothLeftArm = new SmoothVector();
        protected readonly SmoothVector SmoothRightArm = new SmoothVector();
        protected SmoothVector SmoothPosition;
        protected SmoothVector SmoothVelocity;
        protected SmoothRotation SmoothRotation;
        protected SmoothVector SmoothAngularVelocity;
        protected Action<float> WheelYawSetter;
        protected Action<float> WheelPitchSetter;

        protected virtual void Awake()
        {
            rigidbody = gameObject.GetComponent<Rigidbody>();
            // For now, we assume the set position and rotation is equal to the server one.
            // Default velocities are probably empty, but set them anyway.
            SmoothPosition = new SmoothVector(gameObject.transform.position);
            SmoothVelocity = new SmoothVector(rigidbody.velocity);
            SmoothRotation = new SmoothRotation(gameObject.transform.rotation);
            SmoothAngularVelocity = new SmoothVector(rigidbody.angularVelocity);
        }

        protected virtual void FixedUpdate()
        {
            SmoothYaw.FixedUpdate();
            SmoothPitch.FixedUpdate();

            SmoothPosition.FixedUpdate();
            SmoothVelocity.FixedUpdate();
            rigidbody.isKinematic = false; // we should maybe find a way to remove UWE's FreezeRigidBodyWhenFar component...tried removing it but caused a bunch of issues.
            rigidbody.velocity = MovementHelper.GetCorrectedVelocity(SmoothPosition.Current, SmoothVelocity.Current, gameObject, PlayerMovementBroadcaster.BROADCAST_INTERVAL);
            SmoothRotation.FixedUpdate();
            SmoothAngularVelocity.FixedUpdate();
            rigidbody.angularVelocity = MovementHelper.GetCorrectedAngularVelocity(SmoothRotation.Current, SmoothAngularVelocity.Current, gameObject, PlayerMovementBroadcaster.BROADCAST_INTERVAL);
            
            WheelYawSetter(SmoothYaw.SmoothValue);
            WheelPitchSetter(SmoothPitch.SmoothValue);
        }

        internal void SetPositionVelocityRotation(Vector3 remotePosition, Vector3 remoteVelocity, Quaternion remoteRotation, Vector3 remoteAngularVelocity)
        {
            gameObject.SetActive(true);
            SmoothPosition.Target = remotePosition;
            SmoothVelocity.Target = remoteVelocity;
            SmoothRotation.Target = remoteRotation;
            SmoothAngularVelocity.Target = remoteAngularVelocity;
        }

        internal virtual void SetSteeringWheel(float yaw, float pitch)
        {
            SmoothYaw.Target = yaw;
            SmoothPitch.Target = pitch;
        }

        internal virtual void SetArmPositions(Vector3 leftArmPosition, Vector3 rightArmPosition)
        {
            SmoothLeftArm.Target = leftArmPosition;
            SmoothRightArm.Target = rightArmPosition;
        }

        internal virtual void Enter()
        {
            enabled = true;
        }

        public virtual void Exit()
        {
            enabled = false;
        }

        internal abstract void SetThrottle(bool isOn);
    }
}
