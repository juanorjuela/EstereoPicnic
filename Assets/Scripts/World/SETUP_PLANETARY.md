# Planetary Movement System — Setup

## Overview

`PlanetaryController.cs` handles custom gravity toward a planet, surface alignment, and orbital movement by **rotating the planet** (the player stays on the surface). Uses **Rigidbody** only; no CharacterController.

## Step-by-Step Unity Setup

1. **Planet**
   - Use a GameObject that represents the world (e.g. GrassSphere or its parent).
   - Assign its **Transform** as the **Planet** reference in the controller.
   - The planet will be rotated by the script; do **not** add a Rigidbody to it.
   - Ensure it has a Collider (e.g. Sphere or Capsule) for ground and set the **Ground Layer Mask** on the player controller to match.

2. **Player**
   - Add a **Rigidbody** (e.g. mass 1, drag 0 or low, angular drag 0).
   - Optionally freeze rotation on X and Z if you want only script-driven rotation.
   - Add a **CapsuleCollider** (or CylinderCollider) for the character.
   - Add **PlanetaryController** and assign the **Planet** reference.
   - Default gravity is disabled in code (`useGravity = false`).

3. **Input**
   - Uses the existing **InputSystem_Actions** (Player.Move, Player.Jump). No asset assignment needed; the script enables actions in `OnEnable`.

4. **Camera**
   - Make the camera a **child of the player**. It will follow and orient with the player by default.
   - Optionally assign it to **Optional Camera** on the controller for tangent-frame reference (otherwise world forward is used).

5. **Using in `world_movement_test` (or similar)**
   - Disable **WorldRotationController**, **WorldRotationCharacterFollower**, and **WorldRotationCharacterController** on the character.
   - Disable or remove the **CharacterController** on the same GameObject.
   - Add **Rigidbody** and **PlanetaryController** as above and assign the planet (e.g. the world/planet Transform that has the collider).

## Inspector Parameters

| Parameter | Purpose |
|-----------|--------|
| **Planet** | Transform of the planet (required). |
| **Gravity Strength** | Force magnitude toward planet center. |
| **Move Speed** | Planet rotation speed (degrees/sec per unit input). |
| **Rotation Speed** | How fast the player’s up aligns to the surface normal (slerp). |
| **Acceleration / Deceleration Rate** | Smoothing when input is applied or released. |
| **Rotation Damping** | Optional extra smoothing (0 = off). |
| **Jump Force** | Impulse along surface normal when grounded. |
| **Ground Check Distance** | Distance along gravity direction to consider grounded. |
| **Ground Layer Mask** | Layers used for ground detection. |

## Math (Brief)

- **Gravity**: `direction = (planet.position - player.position).normalized`; force = direction × gravityStrength.
- **Surface normal**: `up = (player.position - planet.position).normalized`.
- **Tangent frame**: Right = Cross(up, reference forward); Forward = Cross(right, up). Used for planet rotation axes and alignment.
- **Planet rotation**: Planet rotates around its center around the player’s “right” (pitch) and “forward” (yaw) axes; then the player is moved to the new world position of the same surface point via `MovePosition`.
