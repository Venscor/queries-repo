result = Find_Sanitize_DBX();
result.Add(Find_Sanitize_MSSQL());
result.Add(Find_Sanitize_MYSQL());
result.Add(Find_Sanitize_ODBC());
result.Add(Find_Sanitize_ORACLE()); 
result.Add(Find_Sanitize_PDO());
result.Add(Find_Sanitize_PG());
result.Add(Find_Sanitize_ActiveRecord());
result.Add(Find_General_Sanitize());

CxList methods = Find_Methods();
result.Add(methods.FindByShortName("fix_quotes"));