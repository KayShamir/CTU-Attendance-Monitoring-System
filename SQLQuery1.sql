SELECT 
    s.student_id,
    CASE 
        WHEN a.attendance_time_in IS NOT NULL THEN 1
        WHEN a.student_id IS NULL THEN 2
        ELSE 0
    END AS status
FROM 
    student s
INNER JOIN 
    attendance a ON s.student_id = a.student_id AND a.COURSE_ID = 9 AND a.attendance_date = CAST(GETDATE() AS DATE)