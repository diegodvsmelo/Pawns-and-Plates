public enum TaskType
{
    Service,
    Cooking,
    Operational
}

public enum EmployeeSkillType
{
    Cooking,
    Service,
    Operational,
    Agility
}

public enum TaskOutcomeFlow
{
    None,
    GenerateOrder,
    DeliverOrder,
    GenerateSinkCleaning,
    CleanStructure,
    RepairStructure
}

public enum TaskGeneratorType
{
    Cashier,
    Table,
    Oven,
    Grill,
    Stove,
    Sink,
    Counter,
    GenericOperational
}

public enum StructureState
{
    Available,
    Occupied,
    WaitingForCooking,
    Dirty,
    Eating,
    Broken,
    Disabled
}

public enum MalfunctionMode
{
    None,
    BlockForSeconds,
    PenalizeNextTasks
}

public enum TaskState
{
    Available,
    InProgress,
    ReadyToCollect,
    Completed,
    Expired
}