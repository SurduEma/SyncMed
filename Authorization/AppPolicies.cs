namespace SyncMed.Authorization;

public static class AppPolicies
{
    public const string AdminOnly = nameof(AdminOnly);
    public const string PatientOrAdmin = nameof(PatientOrAdmin);
    public const string DoctorOrAdmin = nameof(DoctorOrAdmin);
    public const string NurseOrAdmin = nameof(NurseOrAdmin);
    public const string StaffOrAdmin = nameof(StaffOrAdmin);
    public const string AnyAuthenticatedRole = nameof(AnyAuthenticatedRole);
}
