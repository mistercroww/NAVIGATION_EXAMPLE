# NAVIGATION_EXAMPLE

Example project on how to “spice up” Unity’s default navigation system.

![Demo](https://i.gyazo.com/bbbc199f266121e48520793559bff950.gif)

This project shows how to push Unity’s NavMeshAgent further with custom animation blending, natural movement, spline-driven paths, and real-time look-at behavior.

---

## ✨ Features

### 🧭 **Natural Movement Mode**

A custom movement system that overrides the default NavMesh rotation and speed behavior:

* Smooth turning using `Quaternion.Slerp`
* Dynamic speed multiplier based on turning angle
* Better acceleration / deceleration control
* More “alive” NPC motion compared to stock NavMeshAgent

---

### 👀 **Smart Look-At System**

NPCs don’t snap or jitter — they smoothly track their next path segment using:

* A dynamic target point updated each frame
* Optional height offset
* `Vector3.SmoothDamp` for natural head movement
* Debug lines to visualize steering and look direction

---

### 🚶 **NPC State Machine (Simple but Effective)**

Two clean states:

* `Idle`
* `Walking`

With:

* Animation speed damping
* Event callbacks when points are reached
* Support for looping or one-shot paths
* Per-point “OnPointReached” event and “OnPathComplete” event

---

### 📍 **Spline-Based Path Editing**

A powerful toolset to author NPC paths using Unity’s Spline package:

* Auto-generate path points along the spline
* Generate from spline knots
* Distribute existing points evenly
* Auto-align each point’s rotation to spline tangent
* Custom inspector tools for quick iteration

Includes a visualizer using a `LineRenderer`:

* Smooth spline preview
* Adjustable resolution
* Optional straight-line mode
* Real-time updates when spline changes

---

### ⚙️ **Editor Tools Included**

Custom Unity Editor window to:

* Generate points
* Align rotations
* Visualize the path
* Clean and rebuild the path list

All tools use Unity’s **Undo** system correctly so nothing breaks your workflow.

---

## 📦 Project Structure

* `NPC_Controller.cs` — Core movement, animation, logic
* `NPC_PathHelper.cs` — Manages path points
* `NPC_PathHelperEditor.cs` — Custom inspector with generation tools
* `NPC_PathVisualizer.cs` — Spline/point visual debug renderer

---

## 🎯 Purpose

This repository exists to give Unity developers a practical example of:

* Enhancing the built-in NavMesh system
* Adding character personality through animation and movement
* Mixing NavMesh with Splines for level design flexibility
* Building clean editor tools for better workflow

---

And yes, if you are wondering, i couldn't bother and just dumped the code on GPT and asked for a readme file hahahah.
This is MIT license, so do whatever you want with the code, extended further if you want, that would be nice :)
