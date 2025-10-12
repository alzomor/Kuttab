import json
import csv

# Read the rules from rules.json
with open('rules.json', 'r', encoding='utf-8') as f:
    data = json.load(f)

# Create CSV file
with open('Quraan_App_Testing_Template.csv', 'w', newline='', encoding='utf-8-sig') as csvfile:
    # Create header row
    header = ['Rule #', 'Rule Name', 'Description']
    
    # Add columns for 10 testers (Pass/Fail + Comments for each)
    for i in range(1, 11):
        header.append(f'Tester {i} - Pass/Fail')
        header.append(f'Tester {i} - Comments')
    
    writer = csv.writer(csvfile)
    writer.writerow(header)
    
    # Add each rule
    rule_number = 1
    for rule in data['rules']:
        rule_name = rule['name']
        
        # For each case in the rule
        for case in rule['cases']:
            description = case['description']
            
            # Create row with rule info and empty cells for testers
            row = [rule_number, rule_name, description]
            
            # Add empty cells for 10 testers (Pass/Fail + Comments)
            for i in range(10):
                row.append('')  # Pass/Fail cell
                row.append('')  # Comments cell
            
            writer.writerow(row)
        
        rule_number += 1

print("Testing template created successfully: Quraan_App_Testing_Template.csv")
