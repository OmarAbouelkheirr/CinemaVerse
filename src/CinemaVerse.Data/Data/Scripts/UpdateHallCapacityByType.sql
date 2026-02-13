-- Optional one-time script: set Halls.Capacity from HallType (TwoD=1, ThreeD=2, IMAX=3, VIP=4, ScreenX=5).
-- Run this if you have existing halls with capacity values that should match the new fixed-per-type rule.
UPDATE Halls
SET Capacity = CASE HallType
    WHEN 1 THEN 80   -- TwoD
    WHEN 2 THEN 72   -- ThreeD
    WHEN 3 THEN 63   -- IMAX
    WHEN 4 THEN 24   -- VIP
    WHEN 5 THEN 56   -- ScreenX
    ELSE 80
END;
