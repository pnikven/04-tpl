namespace JapaneseCrossword.Enums
{
    public enum SolutionStatus
    {
        /// <description>
        /// ���� �� ��������� ����� ����������� (���������� ����, ��� �������, �������� ���� �� ���������� ��� ��������� ����������� ������ � �������� ������)
        /// </description>
        BadInputFilePath,

        /// <description>
        /// ���� �� ��������� ����� ����������� (���������� ����, ��� ������� ��� ��������� ����������� ������ � �������� ������)
        /// </description>
        BadOutputFilePath,

        /// <description>
        /// �������� ��������� �����������
        /// </description>
        IncorrectCrossword,

        /// <description>
        /// ��������� ����� ��������
        /// </description>
        PartiallySolved,

        /// <description>
        /// ��������� ����� ���������
        /// </description>
        Solved
    }
}