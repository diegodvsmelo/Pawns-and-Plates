using UnityEngine;

public class EmployeeCardTester : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EmployeeCardUI employeeCardUI;

    [Header("Test Data")]
    [SerializeField] private EmployeeData testEmployeeData;

    [Header("Options")]
    [SerializeField] private bool refreshOnStart = true;
    [SerializeField] private bool refreshOnValidate = true;

    private void Start()
    {
        if (refreshOnStart)
            ApplyTestData();
    }

    private void OnValidate()
    {
        if (!refreshOnValidate)
            return;

        if (!Application.isPlaying)
            return;

        ApplyTestData();
    }

    [ContextMenu("Apply Test Data")]
    public void ApplyTestData()
    {
        if (employeeCardUI == null)
        {
            Debug.LogWarning("EmployeeCardTester: EmployeeCardUI não foi atribuído.");
            return;
        }

        if (testEmployeeData == null)
        {
            Debug.LogWarning("EmployeeCardTester: EmployeeData de teste não foi atribuído.");
            return;
        }

        employeeCardUI.Setup(testEmployeeData);
    }
}