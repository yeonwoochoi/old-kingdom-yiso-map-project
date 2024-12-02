using System;
using UnityEngine;

namespace Domain.Direction {
    public enum YisoObjectDirection {
        LEFT, RIGHT, UP, DOWN
    }

    public static class YisoObjectDirectionUtils {
        public static YisoObjectDirection ToReverse(this YisoObjectDirection direction) => direction switch {
            YisoObjectDirection.LEFT => YisoObjectDirection.RIGHT,
            YisoObjectDirection.RIGHT => YisoObjectDirection.LEFT,
            YisoObjectDirection.UP => YisoObjectDirection.DOWN,
            YisoObjectDirection.DOWN => YisoObjectDirection.UP,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };

        public static (YisoObjectDirection, YisoObjectDirection) GetSideDirection(this YisoObjectDirection direction) {
            if (direction is YisoObjectDirection.UP or YisoObjectDirection.DOWN)
                return (YisoObjectDirection.LEFT, YisoObjectDirection.RIGHT);

            return (YisoObjectDirection.UP, YisoObjectDirection.DOWN);
        }

        public static YisoObjectDirection ToObjectDirection(this Vector3 moveDir) =>
            ((Vector2) moveDir).ToObjectDirection();
        
        public static YisoObjectDirection ToObjectDirection(this Vector2 moveDir) {
            var (x, y) = (moveDir.x, moveDir.y);
            var (absX, absY) = (Math.Abs(x), Math.Abs(y));
            if (x < 0) {
                if (y > 0) {
                    return absX > y ? YisoObjectDirection.LEFT : YisoObjectDirection.UP;
                }

                return absX > absY ? YisoObjectDirection.LEFT : YisoObjectDirection.DOWN;
            }

            if (x > 0) {
                if (y > 0) {
                    return x > y ? YisoObjectDirection.RIGHT : YisoObjectDirection.UP;
                }

                return x > absY ? YisoObjectDirection.RIGHT : YisoObjectDirection.DOWN;
            }

            if (y > 0) {
                if (x > 0) {
                    return y > x ? YisoObjectDirection.UP : YisoObjectDirection.RIGHT;
                }

                return y > absX ? YisoObjectDirection.UP : YisoObjectDirection.LEFT;
            }

            if (x > 0) {
                return absY > x ? YisoObjectDirection.DOWN : YisoObjectDirection.RIGHT;
            }

            return absY > absX ? YisoObjectDirection.DOWN : YisoObjectDirection.LEFT;
        }

        public static Vector3 ToVector3(this YisoObjectDirection direction) {
            var vector2 = direction.ToVector2();
            return new Vector3(vector2.x, vector2.y);
        }
        
        public static Vector2 ToVector2(this YisoObjectDirection direction) {
            var result = Vector2.zero;
            switch (direction) {
                case YisoObjectDirection.UP:
                    result = new Vector2(0, 1);
                    break;
                case YisoObjectDirection.DOWN:
                    result = new Vector2(0, -1);
                    break;
                case YisoObjectDirection.LEFT:
                    result = new Vector2(-1, 0);
                    break;
                case YisoObjectDirection.RIGHT:
                    result = new Vector2(1, 0);
                    break;
            }

            return result;
        }

        public static bool IsInAngleRange(this YisoObjectDirection direction, YisoObjectDirection other) {
            var angle = other.ToAngle();
            return direction.IsInAngleRange(angle);
        }
        
        public static bool IsInAngleRange(this YisoObjectDirection direction, float angle) {
            return direction switch {
                YisoObjectDirection.LEFT => angle is >= 135 and <= 225,
                YisoObjectDirection.RIGHT => angle is >= 0 and <= 45 or >= 315 and <= 360,
                YisoObjectDirection.UP => angle is >= 45 and <= 135,
                YisoObjectDirection.DOWN => angle is >= 225 and <= 315,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }

        public static (float, float) ToAngleRange(this YisoObjectDirection direction) => direction switch {
            YisoObjectDirection.LEFT => (135f, 225f),
            YisoObjectDirection.RIGHT => (0, 45),
            YisoObjectDirection.UP => (45f, 135f),
            YisoObjectDirection.DOWN => (225f, 315f),
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };

        public static YisoObjectDirection ToDirection(this float angle) {
            if (angle is >= 135 and <= 225) return YisoObjectDirection.LEFT;
            if (angle is >= 0 and <= 45 || (angle >= 315 && angle <= 360)) return YisoObjectDirection.RIGHT;
            if (angle is >= 45 and <= 135) return YisoObjectDirection.UP;
            if (angle is >= 225 and <= 315) return YisoObjectDirection.DOWN;
            throw new ArgumentOutOfRangeException(nameof(angle), angle, null);
        }

        public static float ToAngle(this YisoObjectDirection direction) => direction switch {
            YisoObjectDirection.LEFT => 90,
            YisoObjectDirection.RIGHT => -90,
            YisoObjectDirection.UP => 0,
            YisoObjectDirection.DOWN => 180,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
    }
}