using System.Text;
using OfficeOpenXml;

namespace QueraGradeExtractor;

internal class Program
{
	/// <summary>
	/// Total delay hours for this assignment
	/// </summary>
	private const int TotalDelayHours = 48;

	private class StudentSubmits
	{
		public int Delay { get; set; }
		public int[] Grade { get; }

		public StudentSubmits(int questionCount)
		{
			Delay = 0;
			Grade = new int[questionCount];
		}
	}

	static void Main()
	{
		ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
		// Load sheets
		using var source = new ExcelPackage(new FileInfo("quera.xlsx"));
		File.Delete("grades.xlsx");
		using var destination = new ExcelPackage(new FileInfo("grades.xlsx"));
		ExcelWorksheet queraSheet = source.Workbook.Worksheets[0];
		ExcelWorksheet gradesSheet = destination.Workbook.Worksheets.Add("grades");
		// Get number of questions and initialize students
		int questionCount = (queraSheet.Dimension.Columns - 3) / 5;
		List<(ulong, StudentSubmits)> students = LoadStudents(questionCount);
		// Initialize the destination sheet columns
		gradesSheet.Cells[1, 1].Value = "STD ID";
		for (var i = 2; i <= questionCount + 1; i++)
			gradesSheet.Cells[1, i].Value = "Q" + (i - 1);
		gradesSheet.Cells[1, questionCount + 2].Value = "Delay";
		// Read each user
		for (var row = 3; row <= queraSheet.Dimension.Rows; row++)
		{
			// Try parse the StdID
			if (!ulong.TryParse(ToEnglishNumber(queraSheet.Cells[row, 2].Text), out ulong stdId))
				continue;
			// Search in list
			StudentSubmits? submits = SearchStudentSubmitByID(stdId, students);
			if (submits == null)
				continue;
			// Read questions
			for (var question = 0; question < questionCount; question++)
			{
				// Dont update the delay if user didn't submitted this question
				if (!int.TryParse(queraSheet.Cells[row, 4 + question * 5 + 4].Text, out int delay))
					continue;
				if (delay < 0) // fuckers let the assignment open
					continue;
				delay = (int) Math.Ceiling((double) (100 - delay) / 100 * TotalDelayHours);
				int.TryParse(queraSheet.Cells[row, 4 + question * 5 + 2].Text, out int score);
				submits.Delay = Math.Max(delay, submits.Delay);
				submits.Grade[question] = score;
			}
		}

		// Write students to destination
		for (var i = 0; i < students.Count; i++)
		{
			gradesSheet.Cells[i + 2, 1].Value = students[i].Item1.ToString();
			for (var j = 0; j < questionCount; j++)
				gradesSheet.Cells[i + 2, j + 2].Value = students[i].Item2.Grade[j].ToString();
			gradesSheet.Cells[i + 2, questionCount + 2].Value = students[i].Item2.Delay.ToString();
		}

		// Save and done
		destination.Save();
	}

	private static List<(ulong, StudentSubmits)> LoadStudents(int questionCount)
	{
		string[] studentsString = File.ReadAllLines("students.txt");
		var result = new List<(ulong, StudentSubmits)>(studentsString.Length);
		result.AddRange(studentsString.Select(std => (ulong.Parse(std), new StudentSubmits(questionCount))));
		return result;
	}

	private static string ToEnglishNumber(string input)
	{
		StringBuilder englishNumbers = new(input.Length);
		for (var i = 0; i < input.Length; i++)
		{
			if (char.IsDigit(input[i]))
				englishNumbers.Append(char.GetNumericValue(input, i));
			else
				englishNumbers.Append(input[i]);
		}

		return englishNumbers.ToString();
	}

	private static StudentSubmits? SearchStudentSubmitByID(ulong stdId, IEnumerable<(ulong, StudentSubmits)> students)
	{
		foreach ((ulong id, StudentSubmits submits) in students)
			if (id == stdId)
				return submits;
		return null;
	}
}