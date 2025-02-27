﻿using System.Diagnostics;
using System.Numerics;

//resources for threads to share
BankAccount accountOne = new(250.75f, 1500.65f);
BankAccount accountTwo = new(1250.75f, 4700.65f);

//Mutex implemtations
Mutex savingsOne = new Mutex(false, "Savings One Mutex");
Mutex savingsTwo = new Mutex(false, "Savings Two Mutex");
Mutex checkingOne = new Mutex(false, "Checking One Mutex");
Mutex checkingTwo = new Mutex(false, "Checking Two Mutex");

//booleans to show if a resource is in use
bool soInUse = false;
bool stInUse = false;
bool coInUse = false;
bool ctInUse = false;

//Methods for the threads to run, some will read or write to the same bank account or try to access both accounts 
//Threads 1 through 6 will utilize mutexes
void threadOne(){
    try{
        //Writing and reading shared resource
        savingsOne.WaitOne();
        accountOne.DepsoitSavings(400f);
        Console.WriteLine("Thread 1 has accessed Account one and deposited $400 to savings \nAccount One Savings: $"+accountOne.getSavingsAmount()+"\n");
    }
    finally{
        savingsOne.ReleaseMutex();
    }
}
void threadTwo(){
    try{
        savingsTwo.WaitOne();
        accountTwo.DepsoitSavings(500.01f);
        Console.WriteLine("Thread 2 has accessed account two and deposited $500.01 \nAccount Two Savings: $"+accountTwo.getSavingsAmount()+"\n");
    }
    finally{
        savingsTwo.ReleaseMutex();
    }
}
void threadThree(){
    try{
        savingsOne.WaitOne();
        Console.WriteLine("Thread 3 has accessed account one \nAccount One Savings: $"+accountOne.getSavingsAmount()+"\n");
    }
    finally{
        savingsOne.ReleaseMutex();
    }
}
void threadFour(){
    try{
        savingsTwo.WaitOne();
        Console.WriteLine("Thread 4 has accessed account two\nAccount two savings: $"+accountTwo.getSavingsAmount()+"\nAccount two checking: $"+accountTwo.getCheckingAmount()+"\n");
    }
    finally{
        savingsTwo.ReleaseMutex();
    }
}
void threadFive(){
    try{
        savingsOne.WaitOne();
        accountOne.WithdrawChecking(275);
        Console.WriteLine("Thread 5 has accessed account 1 and subtracted $275 from checking\nAccount one checking: $"+accountOne.getCheckingAmount()+"\n");
    }
    finally{
        savingsOne.ReleaseMutex();
    }
}
//transferring money between accounts with mutexes
void threadSix(){
    try{
        savingsOne.WaitOne();
        checkingTwo.WaitOne();
        accountOne.WithdrawSavings(500);
        accountTwo.DepositChecking(500);
        Console.WriteLine("Thread 6 has accessed accounts 1 and 2 and transferred $500 from account 1 savings to account 2 checking\nAccount one savings: $"+accountOne.getSavingsAmount()+"\nAccount two checking: $"+accountTwo.getCheckingAmount()+"\n");
    }
    finally{
        savingsOne.ReleaseMutex();
        checkingTwo.ReleaseMutex();
    }
}
//Threads seven and eight showcase a deadlock using c# locks
void threadSeven(){
    lock (accountOne){
        accountOne.WithdrawSavings(1000);
        Console.WriteLine("Thread 7 has acquired account one and withdrew $1000 from savings\nAccount 1 savings: $"+accountOne.getSavingsAmount()+"\n");

        //attempting to get account two while still having account one, thread 8 has account two locked
        lock(accountTwo){
            accountTwo.DepsoitSavings(1000);
            Console.WriteLine("Thread 7 has acquired account two and deposited $1000 to savings\nAccount 2 savings: $"+accountTwo.getSavingsAmount()+"\n");
        }
    }
}
void threadEight(){
    lock (accountTwo){
        accountTwo.WithdrawChecking(234.56f);
        Console.WriteLine("Thread 8 has acquired account two and withdrew $234.56 from checking\nAccount 2 checking: $"+accountTwo.getCheckingAmount()+"\n");

        //attempting to get account one while still having account two, thread 7 has account one
        lock(accountOne){
            accountOne.DepositChecking(234.56f);
            Console.WriteLine("Thread 8 has acquired account one and deposited $234.56 to checking\nAccount 1 checking: $"+accountOne.getCheckingAmount()+"\n");
        }
    }
}
//Threads nine and ten will show how to prevent the deadlock by releasing the resource before requesting another
void threadNine(){
    Monitor.Enter(accountOne);
        try{
            accountOne.WithdrawSavings(759f);
            Console.WriteLine("Thread 9 has acquired account one and withdrew $759 from savings\nAccount 1 Savings: $"+accountOne.getSavingsAmount()+"\n");
        }
        finally{
            Monitor.Exit(accountOne);
        }
    
    Monitor.Enter(accountTwo);
        try{
            accountTwo.DepsoitSavings(759f);
            Console.WriteLine("Thread 9 has acquired account two and deposit $759 to savings\nAccount 2 Savings: $"+accountTwo.getSavingsAmount()+"\n");
        }
        finally{
            Monitor.Exit(accountTwo);
        }    
}
void threadTen(){
    Monitor.Enter(accountTwo);
        try{
            accountTwo.WithdrawSavings(15.45f);
            Console.WriteLine("Thread 10 has acquired account two and withdrew $15.45 from savings\nAccount 2 savings: $"+accountTwo.getSavingsAmount()+"\n");
        }
        finally{
            Monitor.Exit(accountTwo);
        }
    Monitor.Enter(accountOne);
        try{
            accountOne.DepsoitSavings(15.45f);
            Console.WriteLine("Thread 10 has acquired account one and deposited $15.45 to savings\nAccount 1 savings: $"+accountOne.getSavingsAmount()+"\n");
        
        }
        finally{
            Monitor.Exit(accountOne);
        }    
}
//Threads 11 and 12 will enter deadlock, which will be resolved by timeout mechanisms
void threadEleven(){
    //accessing savings one
    if(!soInUse){
        soInUse = true;
        Console.WriteLine("Thread 11 has acquired account one savings");
        accountOne.DepsoitSavings(1234.56f);
        Console.WriteLine("Deposited $1234.56 into account one savings\nAccount 1 savings: $"+accountOne.getSavingsAmount()+"\n");
        //not relenquishing control before requesting another resource to cause deadlock
        for(int i=0; i<2; i++){
            if(!stInUse){
                stInUse = true;
                Console.WriteLine("Thread 11 has acquired account two savings");
                accountTwo.WithdrawSavings(1234.56f);
                Console.WriteLine("Withdrew $1234.56 from account 2 savings\nAccount 2 savings: $"+accountTwo.getSavingsAmount()+"\n");
                //relenquish control
                stInUse = false;
                soInUse = false;
            }
            else{
                //sleep  and check one more time before relenquishing control of first resource
                if(i==0){
                    Console.WriteLine("Thread 11 could not access account 2 savings, sleeping and trying again\n");
                    Thread.Sleep(10000);
                }
                else{
                    Console.WriteLine("Thread 11 cannot access account 2 savings, closing thread\n");
                } 
            }
        }
        //relenquish control and end thread
        soInUse = false;
    }  
}
void threadTwelve(){
    //accessing savings one
    if(!stInUse){
        stInUse = true;
        Console.WriteLine("Thread 12 has acquired account two savings");
        accountOne.DepsoitSavings(1.01f);
        Console.WriteLine("Deposited $1.01 into account two savings\nAccount 2 savings: $"+accountTwo.getSavingsAmount()+"\n");
        //not relenquishing control before requesting another resource to cause deadlock
        for(int i=0; i<2; i++){
            if(!soInUse){
                soInUse = true;
                Console.WriteLine("Thread 12 has acquired account one savings");
                accountTwo.WithdrawSavings(1.01f);
                Console.WriteLine("Withdrew $1.01 from account 1 savings\nAccount 1 savings: $"+accountOne.getSavingsAmount()+"\n");
                //relenquish control
                stInUse = false;
                soInUse = false;
            }
            else{
                //sleep for ten seconds and check one more time before relenquishing control of first resource
                if(i==0){
                    Console.WriteLine("Thread 12 could not access account one savings, sleeping and trying again\n");
                    Thread.Sleep(10000);
                }
                else{
                    Console.WriteLine("Thread 12 cannot access account 1 savings, closing thread\n");
                }       
            }
        }
        //relenquish control and end thread
        stInUse = false;
    }  
}

//Creating the threads
Thread t1 = new System.Threading.Thread(new ThreadStart(threadOne));
Thread t2 = new System.Threading.Thread(new ThreadStart(threadTwo));
Thread t3 = new System.Threading.Thread(new ThreadStart(threadThree));
Thread t4 = new System.Threading.Thread(new ThreadStart(threadFour));
Thread t5 = new System.Threading.Thread(new ThreadStart(threadFive));
Thread t6 = new System.Threading.Thread(new ThreadStart(threadSix));
//Deadlock threads
/*Thread t7 = new System.Threading.Thread(new ThreadStart(threadSeven));
Thread t8 = new System.Threading.Thread(new ThreadStart(threadEight));*/
//Preventing deadlock threads
Thread t9 = new System.Threading.Thread(new ThreadStart(threadNine));
Thread t10 = new System.Threading.Thread(new ThreadStart(threadTen));
//timeout mechanism threads (same as t7 and t8)
Thread t11 = new System.Threading.Thread(new ThreadStart(threadEleven));
Thread t12 = new System.Threading.Thread(new ThreadStart(threadTwelve));



//Starting threads, threads end after finishing their respective methods 
t1.Start();
t2.Start();
t3.Start();
t4.Start();
t5.Start();
t6.Start();
//t7.Start();
//t8.Start();
t9.Start();
t10.Start();
t11.Start();
t12.Start();

/*Implement priority queues for threads?*/

//basic bank account class, nothing fancy since the project is about the threading
class BankAccount{
    static int NumOfAccounts =0;
    int AccountID;
    float CheckingAmount;
    float SavingsAmount;

    public BankAccount(float Checking, float Savings){
        NumOfAccounts++;
        AccountID = NumOfAccounts;
        CheckingAmount = Checking;
        SavingsAmount = Savings;
    }

    public void WithdrawChecking(float amount){    
        if(CheckingAmount<amount){
            //Take the difference out of savings and charge an overdraft fee($5.00)
            WithdrawSavings(amount-CheckingAmount + 5.00f);
        }
        CheckingAmount = CheckingAmount - amount;
    }
    public void WithdrawSavings(float amount){
        if(amount>=SavingsAmount){
            SavingsAmount = SavingsAmount - amount;
        }
    }
    public void DepositChecking(float amount){
        CheckingAmount = CheckingAmount + amount;
    }
    public void DepsoitSavings(float amount){
        SavingsAmount = SavingsAmount + amount;
    }
    void SavingsInterest(float APY){
        SavingsAmount = SavingsAmount + (SavingsAmount * APY);
    }
    public float getCheckingAmount(){
        return CheckingAmount;
    }
    public float getSavingsAmount(){
        return SavingsAmount;
    }
    public int getAccountID(){
        return AccountID;
    }
}

