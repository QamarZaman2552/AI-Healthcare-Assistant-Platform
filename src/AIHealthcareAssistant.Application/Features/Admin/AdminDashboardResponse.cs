using System;
using System.Collections.Generic;
using System.Text;

namespace AIHealthcareAssistant.Application.Features.Admin;

public class AdminDashboardResponse
{
    public int TotalPatients { get; set; }

    public int TotalDoctors { get; set; }

    public int TotalAppointments { get; set; }

    public int TodaysAppointments { get; set; }

}
