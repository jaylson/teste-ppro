namespace PartnershipManager.Domain.Enums;

public enum UserStatus
{
    Active = 1,
    Inactive = 2,
    Pending = 3,
    Blocked = 4
}

public enum ClientStatus
{
    Active = 1,
    Inactive = 2,
    Suspended = 3
}

public enum DocumentType
{
    Cpf = 1,
    Cnpj = 2
}

public enum CompanyStatus
{
    Active = 1,
    Inactive = 2,
    Suspended = 3
}

public enum LegalForm
{
    LTDA = 1,
    SA = 2,
    EIRELI = 3,
    MEI = 4,
    SLU = 5,
    Other = 99
}

public enum Language
{
    Portuguese = 1,
    English = 2,
    Spanish = 3
}

public enum Role
{
    SuperAdmin = 1,
    Admin = 2,
    Founder = 3,
    BoardMember = 4,
    Legal = 5,
    Finance = 6,
    HR = 7,
    Employee = 8,
    Investor = 9,
    Viewer = 10
}

public enum ShareholderType
{
    Founder = 1,
    Investor = 2,
    Employee = 3,
    Advisor = 4,
    ESOP = 5,
    Other = 99
}

public enum ShareholderStatus
{
    Active = 1,
    Inactive = 2,
    Pending = 3,
    Exited = 4
}

public enum Gender
{
    Male = 1,
    Female = 2,
    Other = 3,
    NotInformed = 4
}

public enum MaritalStatus
{
    Single = 1,
    Married = 2,
    StableUnion = 3,
    Divorced = 4,
    Widowed = 5
}

public enum ContractStatus
{
    Draft = 1,
    PendingSignatures = 2,
    PartiallySigned = 3,
    Signed = 4,
    Expired = 5,
    Cancelled = 6
}

public enum WorkflowStatus
{
    Pending = 1,
    InProgress = 2,
    Approved = 3,
    Rejected = 4,
    Cancelled = 5
}

public enum VestingGrantStatus
{
    Active = 1,
    Completed = 2,
    Terminated = 3,
    Forfeited = 4
}

public enum ValuationStatus
{
    Draft = 1,
    PendingApproval = 2,
    Approved = 3,
    Superseded = 4
}

public enum AuditAction
{
    Create = 1,
    Update = 2,
    Delete = 3,
    Login = 4,
    Logout = 5,
    PasswordChange = 6,
    StatusChange = 7
}

public enum ShareClassStatus
{
    Active = 1,
    Inactive = 2
}

public enum AntiDilutionType
{
    None = 0,
    FullRatchet = 1,
    WeightedAverage = 2
}

public enum ShareOrigin
{
    Issue = 1,
    Transfer = 2,
    Conversion = 3,
    Grant = 4
}

public enum ShareStatus
{
    Active = 1,
    Cancelled = 2,
    Converted = 3,
    Transferred = 4
}

public enum TransactionType
{
    Issue = 1,
    Transfer = 2,
    Cancel = 3,
    Convert = 4,
    Split = 5,
    ReverseSplit = 6
}
