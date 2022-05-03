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

using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using ExpressionParameter = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.Parameter;
using DriverParameter = VRC.SDKBase.VRC_AvatarParameterDriver.Parameter;

namespace Nivera.VRC.Avatars.BonkStick
{
    public class BonkStickWindow : EditorWindow
    {

        private VRCAvatarDescriptor _descriptor;
        private bool _isSetupBatActivation = true;
        private bool _isUpdateGameObject = true;
        private bool _isUseRightHand = true;
        private bool _isQuest = false;

        private Settings Settings => new Settings(_isSetupBatActivation, _isUpdateGameObject, _isUseRightHand, _isQuest);

        [MenuItem("Tools/Nivera/Bonking Stick Setup")]
        private static void ShowWindow()
        {
            var window = GetWindow<BonkStickWindow>();
            window.titleContent = new GUIContent("Bonking Stick Setup");
            window.Show();
        }

        private void CreateGUI()
        {
            foreach (var descriptor in FindObjectsOfType<VRCAvatarDescriptor>())
            {
                if (descriptor.gameObject.activeSelf)
                {
                    _descriptor = descriptor;
                    break;
                }
            }
        }

        private void OnGUI()
        {
            Animator animator = null;
            if(_descriptor != null)
                animator = _descriptor?.GetComponent<Animator>();
            
            EditorGUILayout.HelpBox($"You can readjust position of the bat after installing by deactivating " +
                                    $"parent constraint on object {Constants.ObjectBonkingBat}, moving and reactivating it(with \"Activate\" button)",
                MessageType.Info);
            EditorGUILayout.Space();
            _descriptor =
                EditorGUILayout.ObjectField("Avatar Descriptor", _descriptor, typeof(VRCAvatarDescriptor), true)
                    as VRCAvatarDescriptor;

            _isSetupBatActivation =
                EditorGUILayout.Toggle(new GUIContent("Setup bat activation?",
                    "Do you want to have toggle layer generated as well?"), _isSetupBatActivation);
            
            _isUpdateGameObject =
                EditorGUILayout.Toggle(new GUIContent("Reinstall game object?", 
                    "Do you want object and it's settings to be reset as well?"), _isUpdateGameObject);
            
            _isQuest =
                EditorGUILayout.Toggle(new GUIContent("Install Quest version", 
                    "Install quest compatible version?"), _isQuest);

            EditorGUI.BeginDisabledGroup(animator == null || !animator.isHuman);
            _isUseRightHand =
                EditorGUILayout.Toggle(new GUIContent("Use Right hand?",
                        "If unchecked left hand will be used to hold the bat"), _isUseRightHand);
            EditorGUI.EndDisabledGroup();

            if(animator != null)
                DrawValidationHelpBoxes(animator);
            
            // Install button

            EditorGUI.BeginDisabledGroup(_descriptor == null || animator == null);
            if (GUILayout.Button("Install"))
            {
                EditorGUILayout.HelpBox($"It is highly recommended to backup your fx layer and avatar before " +
                                        $"installing. It shouldn't break anything, but better safe than sorry.",
                    MessageType.Info);
                Install();
            }
            
            // Remove button
            if (GUILayout.Button("Remove"))
            {
                Remove();
            }

            EditorGUI.EndDisabledGroup();
        }

        private void Install()
        {
            var settings = Settings;

            ExpressionsInstaller.Install(_descriptor, settings);
            MenuInstaller.Install(_descriptor, settings);
            AnimationsInstaller.Install(_descriptor, settings);
            GameObjectInstaller.Install(_descriptor, settings);
        }

        private void Remove()
        {
            var settings = Settings;
            
            ExpressionsInstaller.Remove(_descriptor);
            MenuInstaller.Remove(_descriptor);
            AnimationsInstaller.Remove(_descriptor);
            GameObjectInstaller.Remove(_descriptor);
            AssetDatabase.SaveAssets();
        }

        private void DrawValidationHelpBoxes(Animator animator)
        {
            if (_descriptor == null)
            {
                EditorGUILayout.HelpBox("Specify avatar to install", MessageType.Error);
            }
            else if (animator == null)
            {
                EditorGUILayout.HelpBox("You require animator on avatar!", MessageType.Error);
            }
            else if(!animator.isHuman)
            {
                EditorGUILayout.HelpBox("Avatar is not humanoid. Position of bat won't be adjusted to hand automatically",
                    MessageType.Warning);
            }
        }
    }
}

#endif