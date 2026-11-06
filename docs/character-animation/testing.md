# Character testing checklist

Status: **not run yet**. Repository inspection doesn't prove that gameplay works in Play mode.

## How to use this

Open `Assets/Scenes/Prototype.unity` in Unity 2023.1.22f1. Run these checks once with the capsule
and again after adding the character. Keep the starting stats and scene setup the same so the
comparison is useful. Use a safe test copy for any temporary platforms or obstacles.

For each run, record the branch/commit, date, tester, result and any Console errors.
If something already fails with the capsule, note it as an existing issue.

| Check | What to look for | Capsule baseline | Character |
| --- | --- | --- | --- |
| Idle | Feet on the floor; no drifting or jitter | Not run | Not run |
| Walk | WASD, diagonals, backwards and strafing work | Not run | Not run |
| Sprint | Shift forwards; exhaustion still stops sprinting | Not run | Not run |
| Jump | Standing, walking and sprint jumps; stamina still spent | Not run | Not run |
| Fall and land | Step off an edge; grounded state returns; fall damage still works | Not run | Not run |
| Crouch | Ctrl, movement while crouched, ceiling check, standing again; inspect model squashing | Not run | Not run |
| Roll | Q in different directions; stamina, cooldown and dodge timing unchanged | Not run | Not run |
| Camera | Mouse rotation and pitch; character doesn't block the view unexpectedly | Not run | Not run |
| Collision | Walls, slopes, steps and low ceilings behave as before | Not run | Not run |
| Inventory | Tab opens/closes UI; mouse behaviour and hotbar equipping still work | Not run | Not run |
| Combat | Existing attacks, guard and parry still work; weapon pivot stays correctly placed | Not run | Not run |
| Damage | Model flashes and restores its materials correctly | Not run | Not run |
| Death/respawn | Controls stop; animation doesn't keep walking; scene reloads | Not run | Not run |
| Animation quality | No obvious sliding, snapping, clipping or unwanted root displacement | Not applicable | Not run |
| Console/performance | No new errors, missing references or obvious frame-rate regression | Not run | Not run |

Prototype isn't currently enabled in Build Settings. Record this when testing respawn;
don't mark that check as passed until the scene can actually reload.

## Run notes

No runs recorded yet.
