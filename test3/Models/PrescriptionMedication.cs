using test3.Models;

public class PrescriptionMedication{
    public int PrescriptionID{get;set;}
    public int TakePerWk{get;set;}
    
    public bool SplitMed{get;set;}
    public int Dosage{get;set;}
    public DateTime StartDate{get;set;}
    public DateTime EndDate{get;set;}
    public bool TakeAfterBreakfast{get;set;}
    public bool TakeAfterLunch{get;set;}
    public bool TakeAfterDinner{get;set;}
    public bool TakeBeforeBed{get;set;}
    public string? TakeWhenNeeded{get;set;}
    public Medication Medication{get;set;}
}