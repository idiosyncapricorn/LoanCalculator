def calculate_loan():
    amount = float(input("Enter the loan amount: $"))
    interest_rate = float(input("Enter the interest rate (in percentage): "))
    monthly_payment = float(input("Enter the monthly payment amount: $"))

    # Calculate the minimum monthly payment to cover the interest
    minimum_payment = amount * (interest_rate / 100) / 12

    if monthly_payment < minimum_payment:
        print("Your monthly payment is too low. The minimum monthly payment should be $", round(minimum_payment, 2))
        return

    # Calculate the number of months to pay off the loan
    months = 0
    while amount > 0:
        months += 1
        amount += amount * (interest_rate / 100) / 12  # Add monthly interest
        amount -= monthly_payment  # Subtract monthly payment
        if amount <= 0:
            break

    # Calculate the final payment amount
    final_payment = monthly_payment + amount

    # Calculate the total interest paid
    total_interest = (monthly_payment * months) - amount

    # Print the results
    print("Number of months to pay off the loan:", months)
    print("Final payment amount: $", round(final_payment, 2))
    print("Total interest paid: $", round(total_interest, 2))


# Run the program
calculate_loan()
