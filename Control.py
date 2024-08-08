import subprocess

def start_keylogger():
    print("Starting keylogger...")
    subprocess.run(["./keylogger.exe"], check=True)

def start_wifi_scrambler():
    print("Starting Wi-Fi scrambler...")
    subprocess.run(["./wifi_scrambler.exe"], check=True)

def open_loan_calculator():
    print("Opening loan calculator...")
    subprocess.run(["./loan_calculator.exe"], check=True)

def main():
    print("Choose an option:")
    print("1. Start Keylogger")
    print("2. Start Wi-Fi Scrambler")
    print("3. Open Loan Calculator")
    choice = input("Enter your choice: ")

    if choice == '1':
        start_keylogger()
    elif choice == '2':
        start_wifi_scrambler()
    elif choice == '3':
        open_loan_calculator()
    else:
        print("Invalid choice. Exiting.")

if __name__ == "__main__":
    main()
