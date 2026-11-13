import { useEffect, useRef, useState, useCallback } from "react";
import { AccessibilityInfo, Animated } from "react-native";
import { useFocusEffect } from "expo-router";
import { theme } from "./theme";

export function useReducedMotion() {
  const [reduced, setReduced] = useState(true);
  useEffect(() => {
    let active = true;
    void AccessibilityInfo.isReduceMotionEnabled()
      .then((value) => {
        if (active) setReduced(value);
      })
      .catch(() => {});
    const subscription = AccessibilityInfo.addEventListener(
      "reduceMotionChanged",
      setReduced,
    );
    return () => {
      active = false;
      subscription.remove();
    };
  }, []);
  return reduced;
}

export function AnimatedContent({ children }: { children: React.ReactNode }) {
  const reduced = useReducedMotion();
  const progress = useRef(new Animated.Value(1)).current;
  useFocusEffect(
    useCallback(() => {
      if (reduced) {
        progress.setValue(1);
        return;
      }
      progress.setValue(0);
      const animation = Animated.timing(progress, {
        toValue: 1,
        duration: theme.motion.duration,
        useNativeDriver: true,
      });
      animation.start();
      return () => animation.stop();
    }, [progress, reduced]),
  );
  return (
    <Animated.View
      style={{
        gap: theme.spacing.md,
        opacity: progress,
        transform: [
          {
            translateY: progress.interpolate({
              inputRange: [0, 1],
              outputRange: [theme.motion.distance, 0],
            }),
          },
        ],
      }}
    >
      {children}
    </Animated.View>
  );
}
