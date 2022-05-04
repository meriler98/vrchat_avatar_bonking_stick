[![Generic badge](https://img.shields.io/badge/Unity-2019.4.31f1-informational.svg)](https://unity3d.com/unity/whats-new/2019.4.31)
[![Generic badge](https://img.shields.io/badge/SDK-AvatarSDK3-informational.svg)](https://vrchat.com/home/download)
[![Generic badge](https://img.shields.io/badge/License-MIT-informational.svg)](https://github.com/meriler98/vrchat_avatar_bonking_stick/blob/main/LICENSE)
[![Generic badge](https://img.shields.io/github/downloads/meriler98/vrchat_avatar_bonking_stick/total?label=Downloads)](https://github.com/meriler98/vrchat_avatar_bonking_stick/releases/latest)

# Summary
This is an asset created to add bonking stick to your avatar and have fun bonking people around
(Image with bonking to put)

# Requirements
- VRCSDK3 Avatar 2022.04.21.03.29 or later

# Installation
1. Import the package into unity  
2. Go to the menu(Tools -> Nivera -> Bonking Stick Setup)  
![Step 1](https://raw.githubusercontent.com/meriler98/vrchat_avatar_bonking_stick/main/Images/Install_Step_1.png)  
3. Drag and drop your avatar from scene to the avatar slot  
![Step 2](https://raw.githubusercontent.com/meriler98/vrchat_avatar_bonking_stick/main/Images/Install_Step_2.png)  
4. Click Install  
![Step 3](https://raw.githubusercontent.com/meriler98/vrchat_avatar_bonking_stick/main/Images/Install_Step_3.png)  
5. PROFIT  
  
# Editing
In order to edit position of your bonking bat you need:  
1. Find the object under avatar after installation called "Niv_BonkingBat" and select it  
![Step 1](https://raw.githubusercontent.com/meriler98/vrchat_avatar_bonking_stick/main/Images/Position_editting_Step_1.png)  
2. Activate game object to see it  
![Step 2](https://raw.githubusercontent.com/meriler98/vrchat_avatar_bonking_stick/main/Images/Position_editting_Step_2.png)  
3. Deactivate the Parent Constrant by unchecking "Is Active" option  
![Step 3](https://raw.githubusercontent.com/meriler98/vrchat_avatar_bonking_stick/main/Images/Position_editting_Step_3.png)  
4. Reposition the bat where you need it on scene(Note: it will still be tracking your hand unless you change the constraint source)  
![Step 4](https://raw.githubusercontent.com/meriler98/vrchat_avatar_bonking_stick/main/Images/Position_editting_Step_4.png)  
5.  Activate constraints  
![Step 5](https://raw.githubusercontent.com/meriler98/vrchat_avatar_bonking_stick/main/Images/Position_editting_Step_5.png)  
6. Deactivate game object to avoid it from appearing on avatar's appearence  
![Step 6](https://raw.githubusercontent.com/meriler98/vrchat_avatar_bonking_stick/main/Images/Position_editting_Step_6.png)  
  
# Changelog  

- 1.1  
> Added auto selection of first avatar in descriptor  
> Added quest support option  
> Updated material for bonk particle that works better in vrchat  
