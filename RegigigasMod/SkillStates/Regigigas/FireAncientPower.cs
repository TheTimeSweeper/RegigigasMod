﻿using EntityStates;
using RoR2;
using UnityEngine;
using RoR2.Projectile;

namespace RegigigasMod.SkillStates.Regigigas
{
    public class FireAncientPower : BaseRegiSkillState
    {
        public int rockCount;

        public static float baseDuration = 0.6f;
        public static float baseTimeBetweenRocks = 0.15f;

        public static float damageCoefficient = 4f;
        public static float throwForce = 140f;
        public static float projectileForce = 5f;

        private float throwStopwatch;
        private float timeBetweenRocks;
        private float duration;
        private GameObject projectilePrefab;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = FireAncientPower.baseDuration / this.attackSpeedStat;
            this.timeBetweenRocks = FireAncientPower.baseTimeBetweenRocks / this.attackSpeedStat;
            this.throwStopwatch = this.timeBetweenRocks;

            // cache the prefab to save a little perf
            this.projectilePrefab = Modules.Projectiles.rockProjectile;

            base.PlayAnimation("Gesture, Override", "FireAncientPower", "AncientPower.playbackRate", FireAncientPower.baseDuration);

            Util.PlaySound("sfx_regigigas_rocks_release", this.gameObject);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            base.StartAimMode(0.5f);
            this.throwStopwatch -= Time.fixedDeltaTime;

            this.regigigasController.rockCount = this.rockCount;

            if (this.throwStopwatch <= 0f)
            {
                this.ThrowRock();
            }

            if (base.isAuthority && base.fixedAge >= this.duration && this.rockCount <= 0)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            base.cameraTargetParams.cameraParams = Modules.CameraParams.defaultCameraParams;
            base.characterBody.hideCrosshair = true;

            this.regigigasController.rockCount = 0;
        }

        private void ThrowRock()
        {
            if (this.rockCount <= 0) return;

            this.rockCount--;
            this.throwStopwatch = this.timeBetweenRocks;

            if (base.isAuthority)
            {
                Ray aimRay = base.GetAimRay();

                ProjectileManager.instance.FireProjectile(this.projectilePrefab, 
                    aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), 
                    base.gameObject, 
                    this.damageStat * FireAncientPower.damageCoefficient, 
                    FireAncientPower.projectileForce, 
                    base.RollCrit(), 
                    DamageColorIndex.Default, 
                    null, 
                    FireAncientPower.throwForce);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}