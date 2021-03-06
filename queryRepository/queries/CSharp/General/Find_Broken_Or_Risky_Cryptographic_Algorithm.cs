CxList methods = Find_Methods();
CxList binaryTest = Find_BinaryExpr();
CxList typeRefTest = Find_TypeRef();
CxList stringTest = Find_Strings();
CxList declaratorTest = Find_Declarators();
CxList paramTest = Find_Param();
CxList unknownTest = Find_Unknown_References();
CxList encryptObj = Find_Encrypt();
CxList integerTest = Find_IntegerLiterals();

result.Add(All.FindByType("MD5", false).GetMembersOfTarget().FindByShortName("ComputeHash"));
result.Add(methods.FindByMemberAccess("MD5cryptoserviceprovider.ComputeHash"));
result.Add(All.FindByType("SHA1", false).GetMembersOfTarget().FindByShortName("ComputeHash"));
result.Add(methods.FindByMemberAccess("Sha1cryptoserviceprovider.ComputeHash"));


//Hash Passwords
CxList methodPbkdf = methods.FindByName("KeyDerivation.Pbkdf2");

//build list the elements generated by random
CxList rngGenerator = methods.FindByMemberAccess("RandomNumberGenerator.Create").GetAssignee();
CxList rngUsing = All.FindAllReferences(rngGenerator).GetMembersOfTarget();
CxList methodsGenRandom = rngUsing.FindByShortNames(new List<string>{"GetBytes", "GetInt32", "GetNonZeroBytes", "Fill"});
CxList randomValues = All.FindAllReferences(All.GetParameters(methodsGenRandom));

CxList relevantInt = integerTest.FindByAbstractValue(abstractValue => abstractValue is IntegerIntervalAbstractValue intAbsVal && intAbsVal.UpperIntervalBound >= 10000);
relevantInt.Add(relevantInt.GetAncOfType(typeof(Param)));
relevantInt.Add(unknownTest.FindAllReferences(relevantInt.GetAssignee()));

//binarySafe
CxList unSafeInt = All.NewCxList();
foreach(CxList myBinary in binaryTest){
	BinaryExpr col = myBinary.TryGetCSharpGraph<BinaryExpr>();	
	try{
		if(col.Operator == BinaryOperator.Divide && Convert.ToInt32(col.Left.Text) < 112){
			unSafeInt.Add(myBinary);
			CxList varAssi = myBinary.GetAssignee();
			varAssi.Add(All.FindAllReferences(varAssi));
			unSafeInt.Add(varAssi);
		}
	}catch(Exception e){}
}

foreach(CxList myMethod in methodPbkdf){
	CxList paramPbkdf = paramTest.GetParameters(myMethod);
	foreach(CxList myParam in paramPbkdf){
		Param col = myParam.TryGetCSharpGraph<Param>();
		if(col.Name.Equals("iterationCount")){			
			myParam.Add((unknownTest).FindByFathers(myParam));
			if((myParam * relevantInt).Count == 0){
				result.Add(myMethod);
			}
		}
		else if(col.Name.Equals("salt")){
			CxList testSalt = randomValues * unknownTest.FindByFathers(myParam);
			if( testSalt.Count < 1 ){				
				result.Add(myMethod);			
			}
		}
		else if(col.Name.Equals("numBytesRequested")){
			if( ((unknownTest + binaryTest).FindByFathers(myParam) * unSafeInt).Count > 0){
				result.Add(myMethod);	
			}
		}
	}
}
//Cryptography
CxList objDataProtectionBuilder = encryptObj
	.FindByShortNames(new List<string>{"CngCbcAuthenticatedEncryptorConfiguration", "ManagedAuthenticatedEncryptorConfiguration"});

CxList myDeclarators = declaratorTest
	.FindByShortNames(new List<string>{"EncryptionAlgorithm", "HashAlgorithm", "EncryptionAlgorithmType", "ValidationAlgorithmType"})
	.GetByAncs(objDataProtectionBuilder);

var brokenEncryptionAlgorithmName = new List<string>{"3DES" , "TDES" , "RC2", "RC5"};
CxList brokenEncryptionAlgorithm = stringTest.FindByShortNames(brokenEncryptionAlgorithmName);

var brokenHashAlgorithmName = new List<string>{"MD2", "MD4", "MD5", "SHA-1", "SHA-0" , "Snefru", "GOST"};
CxList brokenHashAlgorithm = stringTest.FindByShortNames(brokenHashAlgorithmName);

String[] brokenEncryptionAlgorithmTypeName = new String[]{"DES" , "RC2" , "TripleDES"};
CxList brokenEncryptionAlgorithmType = (typeRefTest.FindByTypes(brokenEncryptionAlgorithmTypeName)).GetAncOfType(typeof(TypeOfExpr));

String[] brokenValidationAlgorithmTypeName = new String[]{"HMAC" , "HMACMD5" , "HMACRIPEMD160", "MACTripleDES"};
CxList brokenValidationAlgorithmType = (typeRefTest.FindByTypes(brokenValidationAlgorithmTypeName)).GetAncOfType(typeof(TypeOfExpr));

CxList broken = All.NewCxList();
foreach(CxList myDecl in  myDeclarators){
	Declarator coTest = myDecl.TryGetCSharpGraph<Declarator>();
	CxList desc = All.FindByFathers(myDecl);
	if( (brokenEncryptionAlgorithm * desc).Count > 0 
		|| (brokenHashAlgorithm * desc).Count > 0 
		|| (brokenEncryptionAlgorithmType * desc).Count > 0
	|| (brokenValidationAlgorithmType * desc).Count > 0){
		broken.Add(myDecl);
	}
}

result.Add(broken.GetAncOfType(typeof(ObjectCreateExpr)) * objDataProtectionBuilder);