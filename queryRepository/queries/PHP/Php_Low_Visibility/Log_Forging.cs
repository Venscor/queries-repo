CxList Inputs = Find_Interactive_Inputs();
CxList Log = Find_Log_Outputs();
CxList sanitize = Find_General_Sanitize();
	
result = Log.InfluencedByAndNotSanitized(Inputs, sanitize);