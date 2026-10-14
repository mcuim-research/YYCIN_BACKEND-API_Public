using test3.Models;

public class EnvLog{
    public int EnvLogID{get;set;}
    public float Temperature{get;set;}
    public float Humidity{get;set;}
    public DateTime DateTime{get;set;}
    public SmartBox SmartBox{get;set;}
}

public class EnvLogDTO{
    public int smartBoxID{get;set;}
    public int EnvLogID{get;set;}
    public float Temperature{get;set;}
    public float Humidity{get;set;}
}