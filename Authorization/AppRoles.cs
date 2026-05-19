namespace SyncMed.Authorization;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Patient = "Patient";
    public const string Doctor = "Doctor";
    public const string Nurse = "Nurse";

    public const string AdminOnly = Admin;
    public const string PatientOrAdmin = Patient + "," + Admin;
    public const string DoctorOrAdmin = Doctor + "," + Admin;
    public const string NurseOrAdmin = Nurse + "," + Admin;
    public const string StaffOrAdmin = Doctor + "," + Nurse + "," + Admin;
    public const string AnyAuthenticatedRole = Patient + "," + Doctor + "," + Nurse + "," + Admin;
}
