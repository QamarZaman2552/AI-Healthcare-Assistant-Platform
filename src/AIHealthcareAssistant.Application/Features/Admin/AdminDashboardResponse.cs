<<<<<<< HEAD
﻿using System;
using System.Collections.Generic;
using System.Text;

=======
>>>>>>> origin/develop
namespace AIHealthcareAssistant.Application.Features.Admin;

public class AdminDashboardResponse
{
    public int TotalPatients { get; set; }
<<<<<<< HEAD

    public int TotalDoctors { get; set; }

    public int TotalAppointments { get; set; }

    public int TodaysAppointments { get; set; }

=======
    public int TotalDoctors { get; set; }
    public int TotalAppointments { get; set; }
    public int TodayAppointments { get; set; }
    public int ActiveUsers { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
>>>>>>> origin/develop
}
