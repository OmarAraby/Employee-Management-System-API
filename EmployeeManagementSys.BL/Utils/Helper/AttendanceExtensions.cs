using EmployeeManagementSys.DL;
using System;
using System.Collections.Generic;


namespace EmployeeManagementSys.BL
{
    public static class AttendanceExtensions
    {
        public static string GetStatusDisplayName(this Attendance attendance)
        {
            return attendance.Status switch
            {
                AttendanceStatus.Present => "Present",
                AttendanceStatus.Absent => "Absent",
                AttendanceStatus.Late => "Late",
                AttendanceStatus.OnTime => "On Time",
                AttendanceStatus.OnLeave => "On Leave",
                AttendanceStatus.HalfDay => "Half Day",
                _ => "Unknown"
            };
        }
    }
}
