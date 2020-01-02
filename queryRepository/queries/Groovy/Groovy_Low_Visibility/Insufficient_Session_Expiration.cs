/*
We deal with 2 cases:
1. Session expiration in web.xml
2. Session expiration in a Java (or jsp) file
*/

/// Case 1- Session expiration in web.xml

// Find all web.xml data
CxList webxml = All.FindByFileName("*web.xml");

// Find in web.xml the session-timeout, if exists
CxList sessionTimeout = webxml.FindByMemberAccess("SESSION_CONFIG.SESSION_TIMEOUT");
// The session timeout is represented by: SESSION_CONFIG.SESSION_TIMEOUT.TEXT = "<<value>>", so we have to
// find the relevant assign expression
CxList assignSessionTimeout = sessionTimeout.GetAncOfType(typeof(AssignExpr));

// Find all values under session timeout that are "-1"
CxList inXML = webxml.GetByAncs(assignSessionTimeout).FindByShortName(@"""-1""");


/// Case 2 - Session expiration in a Java (or jsp) file

CxList getSession = 
	All.FindByMemberAccess("HttpServletRequest.getSession") +
	All.FindByName("*request.getSession") +  
	All.FindByName("*Request.getSession");
// Find all setMaxInactiveInterval in a session
CxList maxInactiveInterval = 
	All.FindByMemberAccess("HttpSession.setMaxInactiveInterval") +
	getSession.GetMembersOfTarget().FindByShortName("setMaxInactiveInterval");

// All "-1"
CxList minus1 = All.FindByShortName("-1");
// All binary expressions
CxList bin = All.FindByType(typeof(BinaryExpr));

// setMaxInactiveInterval that is influenced by "-1", but with no binary expression in the middle.
// This will find more results than just looking for "-1" as a parametes, but in (very) extreme
// cases might give a false positive (for example abs(-1)), but these are really crazy cases
CxList inJava = maxInactiveInterval.InfluencedByAndNotSanitized(minus1, bin);


// The result contains both cases
result = inXML + inJava;