﻿using System;
using RoR2;
using UnityEngine;
using EntityStates;

namespace RegigigasMod.SkillStates.Regigigas
{
    public class JumpState : BaseState
    {
        private float duration;
        private bool hasInputJump;
        private Animator animator;

        public override void OnEnter()
        {
            base.OnEnter();
            this.animator = base.GetModelAnimator();

            if (this.animator)
            {
                int layerIndex = this.animator.GetLayerIndex("Body");

                this.animator.CrossFadeInFixedTime("AnimatedJump", 0.25f);
                this.animator.Update(0f);

                if (!this.characterBody.HasBuff(Modules.Buffs.slowStartBuff)) this.duration = this.animator.GetNextAnimatorStateInfo(layerIndex).length * 0.5f;
                else this.duration = this.animator.GetNextAnimatorStateInfo(layerIndex).length;
            }

            if (!this.characterBody.HasBuff(Modules.Buffs.slowStartBuff)) this.animator.speed = 2f;
        }

        public override void OnExit()
        {
            base.OnExit();
            if (!this.characterBody.HasBuff(Modules.Buffs.slowStartBuff)) this.animator.speed = 1f;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (this.animator) this.animator.SetBool("isGrounded", false);

            if (base.fixedAge >= this.duration / 2f && base.isAuthority && !this.hasInputJump)
            {
                this.hasInputJump = true;
                base.characterMotor.moveDirection = base.inputBank.moveVector;

                // wax quail
                int itemCount = base.characterBody.inventory.GetItemCount(RoR2Content.Items.JumpBoost);
                float horizontalBonus = 1f;
                float verticalBonus = 1f;
                bool quailJump = false;

                if (itemCount > 0 && base.characterBody.isSprinting)
                {
                    float bonus = base.characterBody.acceleration * base.characterMotor.airControl;
                    if (base.characterBody.moveSpeed > 0f && bonus > 0f)
                    {
                        quailJump = true;
                        float num2 = Mathf.Sqrt(10f * (float)itemCount / bonus);
                        float num3 = base.characterBody.moveSpeed / bonus;
                        horizontalBonus = (num2 + num3) / num3;
                    }
                }

                GenericCharacterMain.ApplyJumpVelocity(base.characterMotor, base.characterBody, horizontalBonus, verticalBonus, false);

                if (quailJump)
                {
                    EffectManager.SpawnEffect(Resources.Load<GameObject>("Prefabs/Effects/BoostJumpEffect"), new EffectData
                    {
                        origin = base.characterBody.footPosition,
                        rotation = Util.QuaternionSafeLookRotation(base.characterMotor.velocity)
                    }, true);
                }

                if (!this.characterBody.HasBuff(Modules.Buffs.slowStartBuff)) this.animator.speed = 1f;
            }
            else
            {
                if (!this.hasInputJump) this.characterMotor.velocity.y = 0f;
            }

            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
            }
        }
    }
}