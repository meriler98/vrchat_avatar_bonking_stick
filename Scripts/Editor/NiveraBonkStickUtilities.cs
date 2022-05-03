/*
    VRChat avatar bonking bat

    Copyright (c) 2022 TheExiledNivera

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
*/

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDKBase;
using Object = UnityEngine.Object;
using ExpressionParameter = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.Parameter;
using DriverParameter = VRC.SDKBase.VRC_AvatarParameterDriver.Parameter;

namespace Nivera.VRC.Avatars.BonkStick
{
    internal static class Constants
    {
        public const string ParamBonkSynced = "Niv_Bonk";
        public const string ParamBatActiveSynced = "Niv_BatActive";
        public const string ParamBonkTriggerLocal = "_Niv_BonkTrigger";
        
        public const string MenuBatActivation = "Activate Bat";
        
        public const string LayerBonkTrigger = "Niv_BonkTrigger";
        public const string LayerBonk = "Niv_Bonk";
        public const string LayerActivation = "Niv_BatActivation";
        
        public const string AnimBonkReset = "Niv_BonkReset";
        public const string AnimBonk = "Niv_Bonk";
        public const string AnimBatOn = "Niv_BatON";
        public const string AnimBatOff = "Niv_BatOFF";
        
        public const string ObjectBonkingBat = "Niv_BonkingBat";

        public const string GeneratedFolderPath = "Assets/Nivera/Generated";
    }

    public static class VRCUtilities
    {
        public static void CreateGenerateFolderIfNeeded()
        {
            if(!AssetDatabase.IsValidFolder(Constants.GeneratedFolderPath))
                AssetDatabase.CreateFolder("Assets/Nivera", "Generated");
        }

        public static void CreateGeneratedFile<T>(T asset, string assetPrefix = "") where T : Object
        {
            CreateGenerateFolderIfNeeded();
            AssetDatabase.CreateAsset(asset, Constants.GeneratedFolderPath + 
                $"/{assetPrefix}{(assetPrefix == String.Empty ? "" : "_")}Generated_{Guid.NewGuid().ToString()}.{GetAssetExtension<T>()}");
        }

        public static string GetAssetExtension<T>() where T : Object
        {
            if (typeof(T) == typeof(AnimationClip))
                return "anim";
            if(typeof(T) == typeof(AnimatorController))
                return "controller";
            return "asset";
        }

        public static void MarkDirty(this Object target)
        {
            EditorUtility.SetDirty(target);
        }

        public static AnimatorControllerLayer CreateDefaultAnimatorLayer(string name)
        {
            var stateMachine = new AnimatorStateMachine { name = name };
            return new AnimatorControllerLayer
            {
                avatarMask = null,
                name = name,
                defaultWeight = 1f,
                stateMachine = stateMachine
            };
        }

        public static void AddLayers(AnimatorController controller, params AnimatorControllerLayer[] layers)
        {
            var layerList = new List<AnimatorControllerLayer>();
            
            if(controller.layers != null)
                layerList.AddRange(controller.layers);

            foreach (var layer in layers)
            {
                AssetDatabase.AddObjectToAsset(layer.stateMachine, controller);

                layerList.Add(layer);
            }

            controller.layers = layerList.ToArray();
            controller.MarkDirty();
        }

        public static void RemoveLayersByName(AnimatorController controller, params string[] names)
        {
            var layerList = new List<AnimatorControllerLayer>();
            
            if(controller.layers != null)
                foreach (var layer in controller.layers)
                {
                    if (names.Contains(layer.name))
                    {
                        foreach (var state in layer.stateMachine.states.Select(x => x.state))
                        {
                            AssetDatabase.RemoveObjectFromAsset(state);
                        }
                        AssetDatabase.RemoveObjectFromAsset(layer.stateMachine);
                    }
                    else
                    {
                        layerList.Add(layer);
                    }
                }
                /*layerList.AddRange(controller.layers.Where(x =>
                    !names.Contains(x.name)));*/

            controller.layers = layerList.ToArray();
            
            controller.MarkDirty();
        }

        public static AnimatorState CreateDefaultState(AnimatorStateMachine stateMachine, string name, Vector3 position,
            AnimationClip clip = null)
        {
            var state = stateMachine.AddState(name, position);
            state.name = name;
            state.motion = clip;
            state.writeDefaultValues = false;

            return state;
        }

        public static AnimatorStateTransition CreateDefaultTransition(AnimatorState source, AnimatorState destinationState,
            bool hasExitTime = false, float exitTime = 1f, float duration = 1f)
        {
            var transition = source.AddExitTransition();
            transition.destinationState = destinationState;
            transition.name = $"-> {destinationState.name}";
            transition.hasExitTime = hasExitTime;
            transition.duration = duration;
            transition.exitTime = exitTime;

            return transition;
        }

        public static AnimatorStateTransition CreateDefaultTransitionWithCondition(AnimatorState source, AnimatorState destinationState,
            string parameter, AnimatorConditionMode mode, float threshold,
            bool hasExitTime = false, float exitTime = 1f, float duration = 1f)
        {
            var transition = CreateDefaultTransition(source, destinationState, hasExitTime, exitTime, duration);
            transition.AddCondition(mode, threshold, parameter);
            
            return transition;
        }

        public static void AddObjectAsset(this Object target, Object objectToAdd, bool hide = false)
        {
            objectToAdd.hideFlags = hide ? HideFlags.HideInHierarchy : HideFlags.None;
            AssetDatabase.AddObjectToAsset(objectToAdd, target);
        }
    }

    internal class Settings
    {
        public bool IsBatActivatable { get; } = true;
        public bool IsUpdateGameObject { get; } = true;

        public bool UseRightHand { get; } = true;

        public bool IsQuest { get; } = false;

        public Settings(bool isBatActivatable, bool isUpdateGameObject, bool useRightHand, bool isQuest)
        {
            IsBatActivatable = isBatActivatable;
            IsUpdateGameObject = isUpdateGameObject;
            UseRightHand = useRightHand;
            IsQuest = isQuest;
        }

        public Settings() { }
    }
    
    internal static class ExpressionsInstaller
    {
        public static void Install(VRCAvatarDescriptor descriptor, Settings settings)
        {
            var expressionParams = descriptor.expressionParameters;
            var parameters = new List<ExpressionParameter>();

            // Create new or remove old ones
            if (expressionParams == null)
            {
                expressionParams = ScriptableObject.CreateInstance<VRCExpressionParameters>();
                VRCUtilities.CreateGeneratedFile(expressionParams, "Expression");
                descriptor.expressionParameters = expressionParams;
            }
            else
            {
                Remove(descriptor);
            }
            
            // Adding old parameters if expressions exists
            if(expressionParams.parameters != null)
                parameters.AddRange(expressionParams.parameters);
            
            // Add new parameters
            parameters.Add(new ExpressionParameter()
            {
                name = Constants.ParamBonkSynced,
                defaultValue = 0f,
                saved = false,
                valueType = VRCExpressionParameters.ValueType.Bool
            });

            if (settings.IsBatActivatable)
            {
                parameters.Add(new ExpressionParameter()
                {
                    name = Constants.ParamBatActiveSynced,
                    defaultValue = 0f,
                    saved = true,
                    valueType = VRCExpressionParameters.ValueType.Bool
                });
            }

            // Apply updates
            expressionParams.parameters = parameters.ToArray();
            EditorUtility.SetDirty(expressionParams);
        }

        public static void Remove(VRCAvatarDescriptor descriptor)
        {
            // If expression parameters exist - remove parameters
            if(descriptor.expressionParameters == null) return;

            var exprParams = descriptor.expressionParameters;
            
            var parameters = new List<ExpressionParameter>();
            
            // Add parameters except those that need to be removed
            parameters.AddRange(
                exprParams.parameters.Where(x => 
                    x.name != Constants.ParamBonkSynced && 
                    x.name != Constants.ParamBatActiveSynced)
                );

            // Update expression parameters
            exprParams.parameters = parameters.ToArray();
            exprParams.MarkDirty();

            Debug.Log("Parameters cleaned up");
        }
    }

    internal static class MenuInstaller
    {
        public static void Install(VRCAvatarDescriptor descriptor, Settings settings)
        {
            // Check if menu needed to be installed
            if (!settings.IsBatActivatable)
            {
                // Prematurely remove old menu if not needed
                if(descriptor.expressionsMenu != null)
                    Remove(descriptor);
                return;
            }

            var menu = descriptor.expressionsMenu;
            var controls = new List<VRCExpressionsMenu.Control>();

            // Create menu if doesn't exist, else remove everything referring
            if (menu == null)
            {
                menu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
                VRCUtilities.CreateGeneratedFile(menu, "Menu");
                descriptor.expressionsMenu = menu;
            }
            else
            {
                Remove(descriptor);
            }

            // Add previous controls
            if(menu.controls != null)
                controls.AddRange(menu.controls);

            // Add new controls
            if (controls.Count < 8)
            {
                controls.Add(new VRCExpressionsMenu.Control()
                {
                    name = Constants.MenuBatActivation,
                    type = VRCExpressionsMenu.Control.ControlType.Toggle,
                    parameter = new VRCExpressionsMenu.Control.Parameter
                        {name = Constants.ParamBatActiveSynced},
                    value = 1f
                });
            }
            else
            {
                Debug.LogError("Too much parameters in menu! You need to free at least 1 item in menu");
            }

            // Apply changes
            menu.controls = controls;
            menu.MarkDirty();
        }

        public static void Remove(VRCAvatarDescriptor descriptor)
        {
            if(descriptor.expressionsMenu == null) return;

            var exprMenu = descriptor.expressionsMenu;
            var controls = new List<VRCExpressionsMenu.Control>();

            // Add menus that were in previous menu, except the option which part of this setup
            if (exprMenu.controls != null)
            {
                controls.AddRange(
                    exprMenu.controls.Where(x => 
                        x.parameter.name != Constants.ParamBatActiveSynced
                        )
                );
            }

            // Apply changes
            exprMenu.controls = controls;
            exprMenu.MarkDirty();
        }
    }

    internal static class AnimationsInstaller
    {
        public static void Install(VRCAvatarDescriptor descriptor, Settings settings)
        {
            AnimatorController animatorFx = GetCustomBaseLayerAnimator(descriptor, VRCAvatarDescriptor.AnimLayerType.FX);

            // Generate new fx layer if doesn't exists
            if (animatorFx == null)
            {
                animatorFx = new AnimatorController();
                VRCUtilities.CreateGeneratedFile(animatorFx, "Animator");
                SetCustomBaseLayerAnimator(descriptor, VRCAvatarDescriptor.AnimLayerType.FX, animatorFx);
            }
            else
            {
                Remove(descriptor);
            }
            
            CreateParameters(animatorFx, settings);

            AddTriggeringLayer(animatorFx);
            AddBonkingLayer(animatorFx);

            if(settings.IsBatActivatable)
                AddBonkingActivationLayer(animatorFx, descriptor.GetComponent<Animator>(), settings);

            animatorFx.MarkDirty();
            AssetDatabase.SaveAssets();
        }

        public static void Remove(VRCAvatarDescriptor descriptor)
        {
            var controller = GetCustomBaseLayerAnimator(descriptor, VRCAvatarDescriptor.AnimLayerType.FX);
            
            // TODO: Remove quest specific animations
            
            VRCUtilities.RemoveLayersByName(controller, 
                Constants.LayerBonkTrigger, Constants.LayerBonk, Constants.LayerActivation);
            
            var assetPath = AssetDatabase.GetAssetPath(controller);
            var objects = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);
            objects.Where(x => x is AnimationClip && (x.name == Constants.AnimBatOn ||
                                                      x.name == Constants.AnimBatOff))
                .ToList().ForEach(AssetDatabase.RemoveObjectFromAsset);
            controller.MarkDirty();

            RemoveParameters(controller);
        }

        #region Parameters
        
        private static void CreateParameters(AnimatorController controller, Settings settings)
        {
            var parameters = new List<AnimatorControllerParameter>();
            
            // Adding old parameters
            if(controller.parameters != null)
                parameters.AddRange(controller.parameters);
            
            parameters.Add(new AnimatorControllerParameter()
            {
                name = Constants.ParamBonkTriggerLocal,
                type = AnimatorControllerParameterType.Float,
                defaultFloat = 0f
            });
            
            parameters.Add(new AnimatorControllerParameter()
            {
                name = Constants.ParamBonkSynced,
                type = AnimatorControllerParameterType.Bool,
                defaultBool = false
            });

            if (settings.IsBatActivatable)
            {
                parameters.Add(new AnimatorControllerParameter()
                {
                    name = Constants.ParamBatActiveSynced,
                    type = AnimatorControllerParameterType.Bool,
                    defaultBool = false
                });
            }

            controller.parameters = parameters.ToArray();
            controller.MarkDirty();
        }
        
        private static void RemoveParameters(AnimatorController controller)
        {
            var parameters = new List<AnimatorControllerParameter>();
            
            parameters.AddRange( controller.parameters.Where(x => 
                    x.name != Constants.ParamBonkTriggerLocal &&
                    x.name != Constants.ParamBonkSynced &&
                    x.name != Constants.ParamBatActiveSynced)
                );

            controller.parameters = parameters.ToArray();
            controller.MarkDirty();
        }
        
        #endregion

        #region Layers

        private static void AddTriggeringLayer(AnimatorController controller)
        {
            var layer = VRCUtilities.CreateDefaultAnimatorLayer(Constants.LayerBonkTrigger);
            var stateMachine = layer.stateMachine;

            controller.AddObjectAsset(stateMachine);

            // Create states
            var idleState = VRCUtilities.CreateDefaultState(stateMachine, "Idle", new Vector3(200f, 0f));
            AddActivationParameterDriver(idleState, Constants.ParamBonkSynced, 0f,
                VRC_AvatarParameterDriver.ChangeType.Set);

            var triggeredState = VRCUtilities.CreateDefaultState(stateMachine, "Triggred", new Vector3(200f, 100f));
            AddActivationParameterDriver(triggeredState, Constants.ParamBonkSynced, 1f,
                VRC_AvatarParameterDriver.ChangeType.Set);
            
            // Creating transitions
            VRCUtilities.CreateDefaultTransitionWithCondition(idleState,
                triggeredState, Constants.ParamBonkTriggerLocal, AnimatorConditionMode.Greater,
                0.5f, duration: 0f);
            
            VRCUtilities.CreateDefaultTransitionWithCondition(triggeredState,
                idleState, Constants.ParamBonkTriggerLocal, AnimatorConditionMode.Less,
                0.5f, duration: 0f);

            // Finishing setup
            stateMachine.defaultState = idleState;
            
            controller.AddLayer(layer);
        }

        private static void AddBonkingLayer(AnimatorController controller)
        {
            var layer = VRCUtilities.CreateDefaultAnimatorLayer(Constants.LayerBonk);
            var stateMachine = layer.stateMachine;
            
            controller.AddObjectAsset(stateMachine);
            
            var resetClip = Resources.Load($"Animations/{Constants.AnimBonkReset}") as AnimationClip;
            var bonkClip = Resources.Load($"Animations/{Constants.AnimBonk}") as AnimationClip;
            
            // Create states
            var resetState = VRCUtilities.CreateDefaultState(stateMachine, "Reset", new Vector3(200f, 0f),
                resetClip);

            var bonkState = VRCUtilities.CreateDefaultState(stateMachine, "Bonk", new Vector3(200f, 100f),
                bonkClip);
            
            // Creating transitions
            VRCUtilities.CreateDefaultTransitionWithCondition(resetState,
                bonkState, Constants.ParamBonkSynced,
                AnimatorConditionMode.If, 0f, duration: 0f);
            
            VRCUtilities.CreateDefaultTransition(bonkState, resetState,
                true, duration: 0f);
            
            // Finishing setup
            stateMachine.defaultState = resetState;
            
            controller.AddLayer(layer);
        }
        
        private static void AddBonkingActivationLayer(AnimatorController controller, Animator animator, Settings settings)
        {
            var layer = VRCUtilities.CreateDefaultAnimatorLayer(Constants.LayerActivation);
            var stateMachine = layer.stateMachine;

            controller.AddObjectAsset(stateMachine);
            
            var onClip = Resources.Load($"Animations/{Constants.AnimBatOn}") as AnimationClip;
            var offClip = Resources.Load($"Animations/{Constants.AnimBatOff}") as AnimationClip;
            
            // Clips override for quest
            if (settings.IsQuest && animator != null)
            {
                onClip = GenerateToggleAnimation(animator, controller, true, settings.UseRightHand);
                offClip = GenerateToggleAnimation(animator, controller, false, settings.UseRightHand);
            }
            
            // Create states
            var offState = VRCUtilities.CreateDefaultState(stateMachine, "OFF", new Vector3(200f, 0f), offClip);
            var onState = VRCUtilities.CreateDefaultState(stateMachine, "ON", new Vector3(200f, 100f), onClip);
            
            // Creating transitions
            VRCUtilities.CreateDefaultTransitionWithCondition(offState, onState,
                Constants.ParamBatActiveSynced,
                AnimatorConditionMode.If, 0f, duration: 0f);
            
            VRCUtilities.CreateDefaultTransitionWithCondition(onState, offState,
                Constants.ParamBatActiveSynced,
                AnimatorConditionMode.IfNot, 0f, duration: 0f);

            // Finishing setup
            stateMachine.defaultState = offState;
            
            controller.AddLayer(layer);
        }

        private static AnimationClip GenerateToggleAnimation(Animator animator, AnimatorController controller,
            bool state, bool rightSide)
        {
            var clip = new AnimationClip();
            var curve = new AnimationCurve();
            
            var hand = rightSide
                ? animator.GetBoneTransform(HumanBodyBones.RightHand)
                : animator.GetBoneTransform(HumanBodyBones.LeftHand);

            var path = AnimationUtility.CalculateTransformPath(hand, animator.transform) + $"/{Constants.ObjectBonkingBat}";

            curve.AddKey(0f, state ? 1f : 0f);
            clip.SetCurve(path, typeof(GameObject), "m_IsActive", curve);
            clip.name = state ? Constants.AnimBatOn : Constants.AnimBatOff;

            controller.AddObjectAsset(clip);
            
            return clip;
        }

        #endregion

        private static AnimatorController GetCustomBaseLayerAnimator(VRCAvatarDescriptor descriptor, VRCAvatarDescriptor.AnimLayerType type)
        {
            VRCAvatarDescriptor.CustomAnimLayer result = new VRCAvatarDescriptor.CustomAnimLayer();

            return descriptor.baseAnimationLayers.First(
                x => x.type == type
                ).animatorController as AnimatorController;
        }

        private static void SetCustomBaseLayerAnimator(VRCAvatarDescriptor descriptor, VRCAvatarDescriptor.AnimLayerType type, AnimatorController controller)
        {
            var fxLayer = new VRCAvatarDescriptor.CustomAnimLayer()
            {
                mask = null,
                type = type,
                isDefault = false,
                animatorController = controller
            };

            var baseLayers = descriptor.baseAnimationLayers;
            for (int i = 0; i < baseLayers.Length; i++)
            {
                if (baseLayers[i].type == type)
                {
                    baseLayers[i] = fxLayer;
                    break;
                }
            }
            descriptor.customizeAnimationLayers = true;
            descriptor.MarkDirty();
        }

        private static void AddActivationParameterDriver(AnimatorState state, string parameterName, float value, VRC_AvatarParameterDriver.ChangeType type)
        {
            var driver = state.AddStateMachineBehaviour<VRCAvatarParameterDriver>();

            driver.localOnly = false;

            var parameter = new DriverParameter
            {
                name = parameterName,
                value = value,
                type = type
            };
            
            driver.parameters = new List<DriverParameter> { parameter };
        }
    }

    internal static class GameObjectInstaller
    {
        public static void Install(VRCAvatarDescriptor descriptor, Settings settings)
        {
            if(!settings.IsUpdateGameObject) return;

            // Cleanup previous one
            Remove(descriptor);
            
            var prefab = settings.IsQuest ? 
                Resources.Load<GameObject>(Constants.ObjectBonkingBat):
                Resources.Load<GameObject>(Constants.ObjectBonkingBat + "_quest");

            var animator = descriptor.GetComponent<Animator>();
            if (animator == null)
            {
                Debug.Log("Can't find applicable animator attached on avatar to pose the bat");
                return;
            }

            var fingerTransform = settings.UseRightHand
                ? animator.GetBoneTransform(HumanBodyBones.RightMiddleDistal)
                : animator.GetBoneTransform(HumanBodyBones.LeftMiddleDistal);

            var handTransform = settings.UseRightHand
                ? animator.GetBoneTransform(HumanBodyBones.RightHand)
                : animator.GetBoneTransform(HumanBodyBones.LeftHand);
            
            var pos = animator.isHuman 
                ? Vector3.down * 0.03f + Vector3.right * ((fingerTransform.position.x - handTransform.position.x) / 2)
                : Vector3.zero;
            var rot = Quaternion.identity;

            var instance = Object.Instantiate(prefab, descriptor.transform);
            instance.name = Constants.ObjectBonkingBat;

            
            // Finalization for quest and ignoring of parent constraint
            if (settings.IsQuest)
            {
                instance.transform.parent = handTransform;
                instance.transform.localPosition = pos;
                instance.transform.rotation = rot;
                return;
            };
            
            var parConstr = instance.GetComponent<ParentConstraint>();
            
            // Quit setting up parent constraint cause we are not humanoid
            if (!animator.isHuman)
            {
                parConstr.constraintActive = false;
                return;
            }

            parConstr.AddSource(new ConstraintSource()
                {weight = 1f, sourceTransform = handTransform});
            parConstr.SetRotationOffset(0, rot.eulerAngles);
            parConstr.SetTranslationOffset(0, pos);

            parConstr.constraintActive = true;
        }

        public static void Remove(VRCAvatarDescriptor descriptor)
        {
            var removeList = new List<Transform>();

            removeList.Add(descriptor.transform.Find(Constants.ObjectBonkingBat));

            var animator = descriptor.GetComponent<Animator>();
            if (animator != null && animator.isHuman)
            {
                removeList.Add(
                    animator.GetBoneTransform(HumanBodyBones.LeftHand).Find(Constants.ObjectBonkingBat)
                    );
                removeList.Add(
                    animator.GetBoneTransform(HumanBodyBones.RightHand).Find(Constants.ObjectBonkingBat)
                );
            }

            removeList.ForEach(x =>
            {
                if(x != null)
                    Object.DestroyImmediate(x.gameObject);
            });
        }
    }
}
#endif
