This is a simple program to track daily transactions for a household.  Point twilio sms to use this as a web hook.  It will save and respond to incoming messages with the format description:amount.  For example, groceries:75.46.  You can view the list of expenditures on the web front end.

curl -i -H "Host: localhost" -H "Content-Type: application/json" -X POST https://localhost:5001/home/create/ -d '{"Name": "Secong", "Description": "Second One", "Amount": "5.32"}' -k
