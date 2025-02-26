using System.Diagnostics;
using System.Numerics;

//resources for threads to share
BankAccount accountOne = new(1, 250.75f, 1500.65f);
BankAccount accountTwo = new(2, 1250.75f, 4700.65f);

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

//Methods for the threads to run, some will read or write to the same bank account and its info simultaenously and cause deadlock
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
//Threads seven and eight showcase a deadlock using locks
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





//Creating the threads
System.Threading.Thread t1 = new System.Threading.Thread(new ThreadStart(threadOne));
System.Threading.Thread t2 = new System.Threading.Thread(new ThreadStart(threadTwo));
System.Threading.Thread t3 = new System.Threading.Thread(new ThreadStart(threadThree));
System.Threading.Thread t4 = new System.Threading.Thread(new ThreadStart(threadFour));
System.Threading.Thread t5 = new System.Threading.Thread(new ThreadStart(threadFive));
System.Threading.Thread t6 = new System.Threading.Thread(new ThreadStart(threadSix));
//Deadlock threads
System.Threading.Thread t7 = new System.Threading.Thread(new ThreadStart(threadSeven));
System.Threading.Thread t8 = new System.Threading.Thread(new ThreadStart(threadEight));
//Preventing deadlock threads
System.Threading.Thread t9 = new System.Threading.Thread(new ThreadStart(threadNine));
System.Threading.Thread t10 = new System.Threading.Thread(new ThreadStart(threadTen));


//Starting threads, threads end after finishing their respective methods 
/*t1.Start();
t2.Start();
t3.Start();
t4.Start();
t5.Start();
t6.Start();

t7.Start();
t8.Start();

t9.Start();
t10.Start();*/

/*Queue to order threads preventing deadlock?
Queue<Thread> threadQueue = new Queue<Thread>(15);
threadQueue.Enqueue(t1);
threadQueue.Enqueue(t2);
threadQueue.Enqueue(t3);
threadQueue.Enqueue(t4);
threadQueue.Enqueue(t5);
threadQueue.Enqueue(t6);
threadQueue.Enqueue(t7);
threadQueue.Enqueue(t8);
threadQueue.Enqueue(t9);
threadQueue.Enqueue(t10);

//this is stupid, defeats point of threading
for(int i=0; i<10;i++){
    System.Threading.Thread temp = threadQueue.Dequeue();
    temp.Start();
    Thread.Sleep(2000);
}*/

//basic bank account class, nothing fancy since the project is about the threading
class BankAccount{
    int AccountID;
    float CheckingAmount;
    float SavingsAmount;

    public BankAccount(int ID, float Checking, float Savings){
        AccountID = ID;
        CheckingAmount = Checking;
        SavingsAmount = Savings;
    }

    public void WithdrawChecking(float amount){
        CheckingAmount = CheckingAmount - amount;
        //return CheckingAmount;
    }
    public void WithdrawSavings(float amount){
        SavingsAmount = SavingsAmount - amount;
        //return SavingsAmount;
    }
    public void DepositChecking(float amount){
        CheckingAmount = CheckingAmount + amount;
        //return CheckingAmount;
    }
    public void DepsoitSavings(float amount){
        SavingsAmount = SavingsAmount + amount;
        //return SavingsAmount;
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

