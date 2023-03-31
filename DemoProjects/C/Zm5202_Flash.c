#include "Zm5202_Flash.h"


uint8_t arr[]={1,3,0,21,233};//ZWAVE  REQUEST

uint8_t SpiTxArr[5];
uint8_t SpiRxArr[10];

uint8_t SerialRxArr[50];

uint8_t addh=0;
uint8_t RxByte = 0xff;
uint32_t Srambuffersize= 2048;

int b=0;
int faultInc=0;
bool SyncDone =false;

bool FlashBusy=true;
bool CRCbusy=true;
bool CRCDone=false;
bool CRC_Failed=true;
bool Txdone = false;

bool contReadCmd = false;
bool CRCcheckInProgress = false;
bool ChipAlreadyProgrammed = false;



uint8_t Sector = 0;
uint8_t LockBitReg = 0;
uint8_t NVRreg = 0;
int nvrcount=0;
ZM_5205_CMD ZM_5205cmd;

uint8_t seton[]= { 1, 9, 0, 19, 255, 2, 39, 4, 37, 11, 21};
uint8_t setoff[]= { 1, 9, 0, 19, 255, 2, 39, 5, 37, 13, 18};
int k = 0;


void TxZwave()
{
	HAL_GPIO_WritePin(ZW_RESET_GPIO_Port,ZW_RESET_Pin,SET);
	HAL_GPIO_WritePin(LED_GREEN_GPIO_Port,LED_GREEN_Pin,SET);
	HAL_GPIO_WritePin(LED_RED_GPIO_Port,LED_RED_Pin,SET);
	HAL_GPIO_WritePin(LED_ORANGE_GPIO_Port,LED_ORANGE_Pin,SET);



	/*if(ZW_Uart.RxState ==  HAL_UART_STATE_READY)
		{
			HAL_UART_Receive_IT(&ZW_Uart,&RxByte,1);
		}*/

	if(k==0)
	{
		//HAL_UART_Receive_IT(&ZW_Uart,&SerialRxArr[0],7);
		//HAL_UART_Transmit_IT(&ZW_Uart,&seton[0],11);
		k=1;
	}else
		if(k==1)
		{
			//HAL_UART_Receive_IT(&ZW_Uart,&SerialRxArr[0],7);
			//HAL_UART_Transmit_IT(&ZW_Uart,&setoff[0],11);
			k=0;
		}
}

void Zm5202Flash_Lib_Init()
{

	HAL_GPIO_WritePin(ZW_RESET_GPIO_Port,ZW_RESET_Pin,SET);
	//HAL_GPIO_WritePin(ZW_IC_CS_ ZW_RESET_GPIO_Port,ZW_RESET_Pin,SET);
	HAL_GPIO_WritePin(LED_GREEN_GPIO_Port,LED_GREEN_Pin,SET);
	HAL_GPIO_WritePin(LED_RED_GPIO_Port,LED_RED_Pin,SET);
	HAL_GPIO_WritePin(LED_ORANGE_GPIO_Port,LED_ORANGE_Pin,SET);

}

void SpiSetup()
{
	HAL_GPIO_WritePin(ZW_RESET_GPIO_Port,ZW_RESET_Pin,RESET);
	//HAL_GPIO_WritePin(Z_CS_GPIO_Port,Z_CS_Pin,RESET);

	ZM_5205cmd.GneralCmd.b0=0xAC;
	ZM_5205cmd.GneralCmd.b1=0x53;
	ZM_5205cmd.GneralCmd.b2=0xAA;
	ZM_5205cmd.GneralCmd.b3=0x55;


	 for(int i=0;i<31;i++)
	 {
		 b=i;
		 HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b0,&SpiRxArr[0],4);
		 HAL_Delay(1);//wait for flash
		 if(SyncDone)
		 {
			 break;
		 }
	 }

	 ReadSignature();
	 CheckState();

	 //Clean();

}
void ReadSignature()
{
	SpiRxArr[0]=0;
	SpiRxArr[1]=0;
	SpiRxArr[2]=0;
	SpiRxArr[3]=0;
	SpiRxArr[4]=0;

	 HAL_Delay(10);//wait for flash

	 ZM_5205cmd.GneralCmd.b0=0x30;
	 ZM_5205cmd.GneralCmd.b1=0x01;
	 ZM_5205cmd.GneralCmd.b2=0xFF;
	 ZM_5205cmd.GneralCmd.b3=0xFF;


	 for(uint8_t i=0;i<7;i++)
	 {
		 b=i;

		 ZM_5205cmd.GneralCmd.b1=i;
		 HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b0,&SpiRxArr[0],4);
		 HAL_Delay(1);

	 }

}

void CheckState()
{

	SpiRxArr[0]=0;
	SpiRxArr[1]=0;
	SpiRxArr[2]=0;
	SpiRxArr[3]=0;
	SpiRxArr[4]=0;

	HAL_Delay(t);//wait for flash
	ZM_5205cmd.GneralCmd.b0=0x7F;
	ZM_5205cmd.GneralCmd.b1=0xFE;
	ZM_5205cmd.GneralCmd.b2=0x00;
	ZM_5205cmd.GneralCmd.b3=0x00;

	//HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b0,&SpiRxArr[0],4);

	HAL_Delay(t);//wait for flash
	 HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b0,&SpiRxArr[0],1);
	 HAL_Delay(t);//wait for flash
	 HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b1,&SpiRxArr[1],1);
	 HAL_Delay(t);
	 HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b2,&SpiRxArr[2],1);
     HAL_Delay(t);//wait for flash
     HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b3,&SpiRxArr[3],1);
     HAL_Delay(t);//wait for flash

}

void Reset()
{

	ZM_5205cmd.GneralCmd.b0=0xFF;
	ZM_5205cmd.GneralCmd.b1=0xFF;
	ZM_5205cmd.GneralCmd.b2=0xFF;
	ZM_5205cmd.GneralCmd.b3=0xFF;

	//HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b0,&SpiRxArr[0],4);
	HAL_Delay(t);
	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b0,&SpiRxArr[0],1);
	HAL_Delay(t);//wait for flash
	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b1,&SpiRxArr[1],1);
	HAL_Delay(t);
	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b2,&SpiRxArr[2],1);
    HAL_Delay(t);//wait for flash
	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b3,&SpiRxArr[3],1);
	HAL_Delay(t);//wait for flash

	for(int i=0;i<65000;i++){}
	HAL_GPIO_WritePin(ZW_RESET_GPIO_Port,ZW_RESET_Pin,SET);


}

void WriteSRAM(uint16_t address ,uint8_t val)
{
	Txdone = false;
	ZM_5205cmd.WriteCmd.b0=0x04;
	ZM_5205cmd.WriteCmd.b1.n=address;//address
	ZM_5205cmd.WriteCmd.b2=val;//data

	addh=ZM_5205cmd.WriteCmd.b1.ar[1];

	SpiRxArr[0]=0;
	SpiRxArr[1]=0;
	SpiRxArr[2]=0;
	SpiRxArr[3]=0;
	SpiRxArr[4]=0;

	HAL_Delay(t);//wait for flash
	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.WriteCmd.b0,&SpiRxArr[0],1);
	HAL_Delay(t);//wait for flash
	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.WriteCmd.b1.ar[1],&SpiRxArr[1],1);
	HAL_Delay(t);
	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.WriteCmd.b1.ar[0],&SpiRxArr[2],1);
    HAL_Delay(t);//wait for flash
    HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.WriteCmd.b2,&SpiRxArr[3],1);
    Txdone = true;
    HAL_Delay(t);//wait for flash*/
}
void ReadSRAM(uint16_t address)
{

	Txdone = false;
	ZM_5205cmd.WriteCmd.b0=0x06;
	ZM_5205cmd.WriteCmd.b1.n=address;//address
	ZM_5205cmd.WriteCmd.b2=0xFF;//data
	addh=ZM_5205cmd.WriteCmd.b1.ar[1];

	SpiRxArr[0]=0;
	SpiRxArr[1]=0;
	SpiRxArr[2]=0;
	SpiRxArr[3]=0;
	SpiRxArr[4]=0;

	HAL_Delay(t);
	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.WriteCmd.b0,&SpiRxArr[0],1);
	HAL_Delay(t);//wait for flash
	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.WriteCmd.b1.ar[1],&SpiRxArr[1],1);
	HAL_Delay(t);
	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.WriteCmd.b1.ar[0],&SpiRxArr[2],1);
    HAL_Delay(t);//wait for flash
    HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.WriteCmd.b2,&SpiRxArr[3],1);
    Txdone = true;
    HAL_Delay(t);//wait for flash*/


}
void WriteFlash(uint8_t sector)
{
	Txdone = false;
	Sector =sector;
	ZM_5205cmd.GneralCmd.b0=0x20;
	ZM_5205cmd.GneralCmd.b1=sector;
	ZM_5205cmd.GneralCmd.b2=0xFF;
	ZM_5205cmd.GneralCmd.b3=0xFF;

	SpiRxArr[0]=0;
	SpiRxArr[1]=0;
	SpiRxArr[2]=0;
	SpiRxArr[3]=0;
	SpiRxArr[4]=0;

	HAL_Delay(t);
	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b0,&SpiRxArr[0],1);
	HAL_Delay(t);//wait for flash
	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b1,&SpiRxArr[1],1);
	HAL_Delay(t);
	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b2,&SpiRxArr[2],1);
	HAL_Delay(t);//wait for flash
	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b3,&SpiRxArr[3],1);
	Txdone = true;
	HAL_Delay(t);//wait for flash*/

	FlashBusy=true;
	do
	{
		CheckState();
		//faultInc++;
		//if(faultInc>1000)
		//{
				//char string[5]="Fault";
				//HAL_UART_Transmit_IT(&ZW_Uart,&string[0],5);
		//	faultInc=0;
		//}
		//HAL_Delay(10);
	}
	while(FlashBusy);

	//HAL_Delay(200);//wait for flash*/


}
void ReadFlash(uint8_t sector)
{
	Txdone = false;
	Sector =sector;
	ZM_5205cmd.GneralCmd.b0=0x10;
	ZM_5205cmd.GneralCmd.b1=sector;
	ZM_5205cmd.GneralCmd.b2=0xFF;
	ZM_5205cmd.GneralCmd.b3=0xFF;

	SpiRxArr[0]=0;
	SpiRxArr[1]=0;
	SpiRxArr[2]=0;
	SpiRxArr[3]=0;
	SpiRxArr[4]=0;

	HAL_Delay(t);

	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b0,&SpiRxArr[0],1);
	HAL_Delay(t);//wait for flash
	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b1,&SpiRxArr[1],1);
	HAL_Delay(t);
	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b2,&SpiRxArr[2],1);
    	HAL_Delay(t);//wait for flash
    	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b3,&SpiRxArr[3],1);
    	Txdone = true;
	HAL_Delay(t);//wait for flash


}
void ContinueWrite(uint8_t val0,uint8_t val1,uint8_t val2)//flash or SRAM depends on what was executed last
{
	Txdone = false;

	ZM_5205cmd.GneralCmd.b0=0x80;
	ZM_5205cmd.GneralCmd.b1=val0;
	ZM_5205cmd.GneralCmd.b2=val1;
	ZM_5205cmd.GneralCmd.b3=val2;

	SpiRxArr[0]=0;
	SpiRxArr[1]=0;
	SpiRxArr[2]=0;
	SpiRxArr[3]=0;
	SpiRxArr[4]=0;

	HAL_Delay(t);
	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b0,&SpiRxArr[0],1);
	HAL_Delay(t);//wait for flash
	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b1,&SpiRxArr[1],1);
	HAL_Delay(t);
	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b2,&SpiRxArr[2],1);
	HAL_Delay(t);//wait for flash
	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b3,&SpiRxArr[3],1);
	Txdone = true;
	HAL_Delay(t);//wait for flash*/
}
void ContinueRead()//flash or SRAM
{
	Txdone = false;

	ZM_5205cmd.GneralCmd.b0=0xA0;
	ZM_5205cmd.GneralCmd.b1=0;
	ZM_5205cmd.GneralCmd.b2=0;
	ZM_5205cmd.GneralCmd.b3=0;

	SpiRxArr[0]=0;
	SpiRxArr[1]=0;
	SpiRxArr[2]=0;
	SpiRxArr[3]=0;
	SpiRxArr[4]=0;

	HAL_Delay(t);
	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b0,&SpiRxArr[0],1);
	HAL_Delay(t);//wait for flash
	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b1,&SpiRxArr[1],1);
	HAL_Delay(t);
	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b2,&SpiRxArr[2],1);
	HAL_Delay(t);//wait for flash
	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b3,&SpiRxArr[3],1);
	Txdone = true;
	contReadCmd =true;
	HAL_Delay(t);//wait for flash*/
}

void EraseChip()
{

	ZM_5205cmd.GneralCmd.b0=0x0A;
	ZM_5205cmd.GneralCmd.b1=0x00;
	ZM_5205cmd.GneralCmd.b2=0x00;
	ZM_5205cmd.GneralCmd.b3=0x00;

	SpiRxArr[0]=0;
	SpiRxArr[1]=0;
	SpiRxArr[2]=0;
	SpiRxArr[3]=0;
	SpiRxArr[4]=0;

	HAL_Delay(t);
	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b0,&SpiRxArr[0],1);
	HAL_Delay(t);//wait for flash
	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b1,&SpiRxArr[1],1);
	HAL_Delay(t);
	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b2,&SpiRxArr[2],1);
	HAL_Delay(t);//wait for flash
	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b3,&SpiRxArr[3],1);
	HAL_Delay(t);//wait for flash

	FlashBusy=true;
	
	 do
	 {
	   CheckState();
	 }
	 while(FlashBusy);
}
void EraseSector(uint8_t sector)
{
	Txdone = false;
	Sector =sector;
	ZM_5205cmd.GneralCmd.b0=0x0B;
	ZM_5205cmd.GneralCmd.b1=sector;
	ZM_5205cmd.GneralCmd.b2=0xFF;
	ZM_5205cmd.GneralCmd.b3=0xFF;

	SpiRxArr[0]=0;
	SpiRxArr[1]=0;
	SpiRxArr[2]=0;
	SpiRxArr[3]=0;
	SpiRxArr[4]=0;

	HAL_Delay(t);

	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b0,&SpiRxArr[0],1);
	HAL_Delay(t);//wait for flash
	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b1,&SpiRxArr[1],1);
	HAL_Delay(t);
	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b2,&SpiRxArr[2],1);
	HAL_Delay(t);//wait for flash
	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b3,&SpiRxArr[3],1);
	Txdone = true;
	HAL_Delay(t);//wait for flash*/

	FlashBusy=true;
	do
	{
		CheckState();
	}
	while(FlashBusy);
}

void DisableEoosMode()
{
	Txdone = false;

	ZM_5205cmd.GneralCmd.b0=0xD0;
	ZM_5205cmd.GneralCmd.b1=0XFF;
	ZM_5205cmd.GneralCmd.b2=0xFF;
	ZM_5205cmd.GneralCmd.b3=0xFF;

	SpiRxArr[0]=0;
	SpiRxArr[1]=0;
	SpiRxArr[2]=0;
	SpiRxArr[3]=0;
	SpiRxArr[4]=0;

	HAL_Delay(t);

	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b0,&SpiRxArr[0],1);
	HAL_Delay(t);//wait for flash
	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b1,&SpiRxArr[1],1);
	HAL_Delay(t);
	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b2,&SpiRxArr[2],1);
	HAL_Delay(t);//wait for flash
	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b3,&SpiRxArr[3],1);
	Txdone = true;
	HAL_Delay(t);//wait for flash
}
void ClearAutoProg1()
{
	Txdone = false;


	ZM_5205cmd.GneralCmd.b0=0xF0;
	ZM_5205cmd.GneralCmd.b1=0x08;
	ZM_5205cmd.GneralCmd.b2=0xFF;
	ZM_5205cmd.GneralCmd.b3=254;

	SpiRxArr[0]=0;
	SpiRxArr[1]=0;
	SpiRxArr[2]=0;
	SpiRxArr[3]=0;
	SpiRxArr[4]=0;

	HAL_Delay(5);

	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b0,&SpiRxArr[0],1);
	HAL_Delay(5);//wait for flash
	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b1,&SpiRxArr[1],1);
	HAL_Delay(5);
	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b2,&SpiRxArr[2],1);
	HAL_Delay(5);//wait for flash
	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b3,&SpiRxArr[3],1);
	Txdone = true;
	HAL_Delay(5);//wait for flash

	FlashBusy=true;
	do
    	{
		CheckState();
	}
	while(FlashBusy);
}

void WriteLockBits(uint8_t LockBitReg_, uint8_t value )
{
	Txdone = false;

	ZM_5205cmd.GneralCmd.b0=0xF0;
	ZM_5205cmd.GneralCmd.b1=LockBitReg_;
	ZM_5205cmd.GneralCmd.b2=0xFF;
	ZM_5205cmd.GneralCmd.b3=value;
	LockBitReg = LockBitReg_;

	SpiRxArr[0]=0;
	SpiRxArr[1]=0;
	SpiRxArr[2]=0;
	SpiRxArr[3]=0;
	SpiRxArr[4]=0;

	HAL_Delay(5);

	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b0,&SpiRxArr[0],1);
	HAL_Delay(5);//wait for flash
	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b1,&SpiRxArr[1],1);
	HAL_Delay(5);
	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b2,&SpiRxArr[2],1);
	HAL_Delay(5);//wait for flash
	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b3,&SpiRxArr[3],1);
	Txdone = true;
	HAL_Delay(5);//wait for flash
}
void ReadLockBits(uint8_t LockBitReg_ )
{
	Txdone = false;

	LockBitReg = LockBitReg_;
	ZM_5205cmd.GneralCmd.b0=0xF1;
	ZM_5205cmd.GneralCmd.b1=LockBitReg;
	ZM_5205cmd.GneralCmd.b2=0xFF;
	ZM_5205cmd.GneralCmd.b3=0xFF;

	SpiRxArr[0]=0;
	SpiRxArr[1]=0;
	SpiRxArr[2]=0;
	SpiRxArr[3]=0;
	SpiRxArr[4]=0;

	HAL_Delay(5);

	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b0,&SpiRxArr[0],1);
	HAL_Delay(5);//wait for flash
	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b1,&SpiRxArr[1],1);
	HAL_Delay(5);
	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b2,&SpiRxArr[2],1);
	HAL_Delay(5);//wait for flash
	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b3,&SpiRxArr[3],1);
	Txdone = true;
	HAL_Delay(5);//wait for flash
}
void ReadAllLockBits()
{
	for(int i=0;i<9;i++)
	{
	  ReadLockBits(i);
	}
}
void WriteNVR(uint8_t addr, uint8_t Data)
{
	Txdone = false;
		ZM_5205cmd.GneralCmd.b0=0xFE;
		ZM_5205cmd.GneralCmd.b1=0x00;
		ZM_5205cmd.GneralCmd.b2=addr;
		ZM_5205cmd.GneralCmd.b3=Data;

		SpiRxArr[0]=0;
		SpiRxArr[1]=0;
		SpiRxArr[2]=0;
		SpiRxArr[3]=0;
		SpiRxArr[4]=0;

		HAL_Delay(5);
		HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b0,&SpiRxArr[0],1);
		HAL_Delay(5);//wait for flash
		HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b1,&SpiRxArr[1],1);
		HAL_Delay(5);
		HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b2,&SpiRxArr[2],1);
		HAL_Delay(5);//wait for flash
		HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b3,&SpiRxArr[3],1);
		Txdone = true;
		HAL_Delay(5);//wait for flash

		FlashBusy=true;
		do
	    {
			CheckState();
		}
		while(FlashBusy);
}
void ReadNVR(uint8_t NvrReg_ )//valid 0X09 to 0XFF
{
	Txdone = false;

	NVRreg = NvrReg_;
	ZM_5205cmd.GneralCmd.b0=0xF2;
	ZM_5205cmd.GneralCmd.b1=0x00;
	ZM_5205cmd.GneralCmd.b2=NVRreg;
	ZM_5205cmd.GneralCmd.b3=0xFF;

	SpiRxArr[0]=0;
	SpiRxArr[1]=0;
	SpiRxArr[2]=0;
	SpiRxArr[3]=0;
	SpiRxArr[4]=0;

	HAL_Delay(t);

	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b0,&SpiRxArr[0],1);
	HAL_Delay(t);//wait for flash
	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b1,&SpiRxArr[1],1);
	HAL_Delay(t);
	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b2,&SpiRxArr[2],1);
	HAL_Delay(t);//wait for flash
	HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b3,&SpiRxArr[3],1);
	Txdone = true;
	HAL_Delay(t);//wait for flash
}
void WriteNVRAll()
{
	for(int i=9;i<255;i++)
	{
		//SetNvr(i,0);
		//SetNvr(i,nvrBytesZm5202test[i-9]);
		WriteNVR(i,nvrBytesZm5202[i-9]);
		//SetNvr(i,nvrBytesRazBerry[i-9]);

	}



}
void ReadNVRAll()
{
	nvrcount=0;
	for(int i=9;i<255;i++)
	{
		ReadNVR(i);
	}

	if(nvrBytesZm5202[119]==1)
	{
		//ChipAlreadyProgrammed =true;//TODO see if this should be used "Currently sending a command to the controller, and checking for a response"
	}

	nvrBytesZm5202[9]=1;//RS232 pins swap = NO swap
	nvrBytesZm5202[10]=4;//Chip select pin is on p0.4

	nvrBytesZm5202[15]=2;//NVMT TYPE

	nvrBytesZm5202[16]=1;//NVMS SIZE =256kb/2Mb
	nvrBytesZm5202[17]=0;//NVMS SIZE

	nvrBytesZm5202[18]=1;//NVMP SIZE = 256 or bigger (page size)
	nvrBytesZm5202[19]=0;//NVMP SIZE



	//the NVR default values can change per given zm5202 module as the calibration values are changed by the manufacturer with calibration


	//E	0x0D3CDE	867550kHz
	//U	0x0DEB7A	912250kHz
	//H	0x0E1578	923000kHz

	if(barr[1226]==250)//US FILE
	{
		nvrBytesZm5202[11]=0x0D;//SAW CENTER FREQUENCY = 912250kHz
		nvrBytesZm5202[12]=0xEB;
		nvrBytesZm5202[13]=0x7A;

		nvrBytesZm5202[14]=0x08;//Saw BandWidht = 8Mhz for the US 0x05 for EU and 7 for H


	}


	uint16_t crc16 = ccittCrc16(0x1D0F, &nvrBytesZm5202[7],110);
	nvrBytesZm5202[117]=(unsigned char)((crc16>>8)&0x00FF);
	nvrBytesZm5202[118]=(unsigned char)(crc16&0x00FF);
	nvrBytesZm5202[119] =1;





}


void UpdateTxPowerSettings()
{
	//DEFAULT
/*	barr[32690]  =0x3F;
	barr[32691]  =0x3F;
	barr[32692]  =0x3F;
	barr[32693]  =0x04;
	barr[32694]  =0x04;
	barr[32695]  =0x04;*/
}

void WriteCRC()
{

//CHECKSUM derived from Equinox software

	EraseSector(63);

	if(barr[1226]==69)//EU FILE
	{
		if(barr[32690]  ==0x3F)//TX power setting set to default
		{
			WriteSRAM(2044,0x83);
			WriteFlash(63);//last sector

			WriteSRAM(2045,0x05);
			WriteFlash(63);

			WriteSRAM(2046,0x1B);
			WriteFlash(63);

			WriteSRAM(2047,0x27);
			WriteFlash(63);
		}
		else
		{

		WriteSRAM(2044,0x62);
		WriteFlash(63);//last sector

		WriteSRAM(2045,0x17);
		WriteFlash(63);

		WriteSRAM(2046,0x8F);
		WriteFlash(63);

		WriteSRAM(2047,0x53);
		WriteFlash(63);
		}
	}

	if(barr[1226]==250)//US FILE
	{
			if(barr[32690]  ==0x3F)//TX power setting set to default
			{
				    WriteSRAM(2044,0x34);
					WriteFlash(63);//last sector

					WriteSRAM(2045,0xDD);
					WriteFlash(63);

					WriteSRAM(2046,0xD9);
					WriteFlash(63);

					WriteSRAM(2047,0xC5);
					WriteFlash(63);
			}
			else
			{

				WriteSRAM(2044,0xD5);
				WriteFlash(63);//last sector

				WriteSRAM(2045,0xCF);
				WriteFlash(63);

				WriteSRAM(2046,0x4D);
				WriteFlash(63);

				WriteSRAM(2047,0xB1);
				WriteFlash(63);
			}
	}


}
void RunCRC()//takes 22ms
{
		ZM_5205cmd.GneralCmd.b0=0xC3;
		ZM_5205cmd.GneralCmd.b1=0;
		ZM_5205cmd.GneralCmd.b2=0;
		ZM_5205cmd.GneralCmd.b3=0;

		SpiRxArr[0]=0;
		SpiRxArr[1]=0;
		SpiRxArr[2]=0;
		SpiRxArr[3]=0;
		SpiRxArr[4]=0;


		//HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b0,&SpiRxArr[0],4);



		HAL_Delay(t);

		HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b0,&SpiRxArr[0],1);
		HAL_Delay(t);//wait for flash
		HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b1,&SpiRxArr[1],1);
		HAL_Delay(t);
		HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b2,&SpiRxArr[2],1);
		HAL_Delay(t);//wait for flash
		HAL_SPI_TransmitReceive_IT(&ZW_Spi,&ZM_5205cmd.GneralCmd.b3,&SpiRxArr[3],1);

		HAL_Delay(t);//wait for flash

		HAL_Delay(100);

		CRCbusy=true;
		CRC_Failed=true;
		do
	        {
			CheckState();
		}
		while(CRCbusy && !CRCDone);
		HAL_Delay(100);
		CRCcheckInProgress =true;
		CheckState();
		CRCcheckInProgress =false;



}

void WriteZwaveCodeToFlash()
{

		//uint8_t sector=0;

	for(int j=0;j<64;j++)
	{
		EraseSector(j);
		HAL_Delay(10);
	}


		//uint8_t jmax=36;
		uint8_t jmax=(barr_length/Srambuffersize)+1;
		int lastbufsize = ((jmax*Srambuffersize)-barr_length);

		for(int j=0;j<jmax;j++)
		{
			HAL_GPIO_TogglePin(LED_ORANGE_GPIO_Port,LED_ORANGE_Pin);
			//HAL_UART_Transmit_IT(&huart2,&j,1);//tjaart send progress
			EraseSector(j);

			if(j==(jmax-1))
			{

				//Srambuffersize = Srambuffersize - 1717;
				Srambuffersize = Srambuffersize-lastbufsize;

			}
			else
			{
				Srambuffersize =2048;
			}
			uint8_t buffer[Srambuffersize];


			uint16_t nrL_FF=0;
			uint16_t nrT_FF=0;
			uint16_t t1=0;

			for(uint16_t k=0;k<Srambuffersize;k++)
			{
				if(buffer[k] ==255)
				{
					nrL_FF++;
				}
				else
				{
					break;
				}
			}


			for(uint16_t k=Srambuffersize-1;k>0;k--)
			{
				if(buffer[k] ==255)
				{
					nrT_FF++;
				}
				else
			    {
					break;
				}
			}

			t1= (Srambuffersize-nrL_FF-nrT_FF)%3;

			if(nrL_FF==Srambuffersize)
			{
				//t1=0;
				//nrL_FF=0;
				//nrT_FF  =2048;


				//j--;
				//jmax--;
			}
		    else
		    {
				if(t1==0)
				{


					WriteSRAM(nrL_FF,barr[((j*2048)+nrL_FF)]);

					for(uint16_t i=nrL_FF+1;i<Srambuffersize - nrT_FF;i+=3)//the size of one sector written to SRAM for transfer
					 {

							uint8_t b0 = barr[ i+(j*2048)];
							uint8_t b1  =barr[(i+1)+(j*2048)];
							uint8_t b2= barr[(i+2)+(j*2048)];
							ContinueWrite(b0,b1,b2);


					 }
					WriteFlash(j);

				}
				else
				{
					do
					{

						if(barr[((j*2048)+nrL_FF)]==255)
						{
							nrL_FF++;
							t1--;
						}
						else
						{

							WriteSRAM(nrL_FF,barr[((j*2048)+nrL_FF)]);
							WriteFlash(j);
							nrL_FF++;
							t1--;
						}

					}while(t1>0);


					WriteSRAM(nrL_FF,barr[((j*2048)+nrL_FF)]);

					for(uint16_t i=nrL_FF+1;i<Srambuffersize - nrT_FF;i+=3)//the size of one sector written to SRAM for transfer
					 {

							uint8_t b0 = barr[ i+(j*2048)];
							uint8_t b1  =barr[(i+1)+(j*2048)];
							uint8_t b2= barr[(i+2)+(j*2048)];
							ContinueWrite(b0,b1,b2);
					 }

					 WriteFlash(j);

				}


		    }

		}






}
void WriteZwaveCodeToFlash1()
{

	for(int j=0;j<64;j++)
	{
		EraseSector(j);
	}

	for(int j=0;j<36;j++)
	{
		//EraseSector(j);
		// HAL_UART_Transmit_IT(&ZW_Uart,&j,1);

		for(uint16_t i=0;i<Srambuffersize;i++)
		{
			if(((j*Srambuffersize)+i)<72011)
			{
				if(barr[(j*Srambuffersize)+i]!=255)
				{
						WriteSRAM((i),barr[(j*Srambuffersize)+i]);
						WriteFlash(j);
				}

			}

		}


	}


}
void ReadAllFlash()
{

  int rmax = (barr_length/3)+1;

	ReadFlash(0);
	for(int i=0;i<rmax;i++)
	{
		ContinueRead();
	}
}

void Test()
{
	HAL_GPIO_WritePin(ZW_RESET_GPIO_Port,ZW_RESET_Pin,SET);
	HAL_GPIO_WritePin(LED_GREEN_GPIO_Port,LED_GREEN_Pin,SET);
	HAL_GPIO_WritePin(LED_RED_GPIO_Port,LED_RED_Pin,SET);
	HAL_GPIO_WritePin(LED_ORANGE_GPIO_Port,LED_ORANGE_Pin,SET);


	SpiSetup();

	//WriteSRAM(0,2);
	//WriteFlash(0);
	WriteCRC();
	RunCRC();




	/*ReadFlash(63);
	for(int i=0;i<679;i++)
	{
		ContinueRead();
	}
	ContinueRead();
	ContinueRead();
	ContinueRead();
	ContinueRead();*/

	Reset();
	HAL_GPIO_WritePin(LED_GREEN_GPIO_Port,LED_GREEN_Pin,RESET);
	HAL_GPIO_WritePin(LED_RED_GPIO_Port,LED_RED_Pin,SET);
	if(!CRC_Failed)
	{
		HAL_GPIO_WritePin(LED_RED_GPIO_Port,LED_RED_Pin,RESET);
	}


}


void ProgramZM5202Module()
{


	HAL_GPIO_WritePin(ZW_RESET_GPIO_Port,ZW_RESET_Pin,SET);
	HAL_GPIO_WritePin(LED_GREEN_GPIO_Port,LED_GREEN_Pin,RESET);
	HAL_GPIO_WritePin(LED_RED_GPIO_Port,LED_RED_Pin,RESET);
	HAL_GPIO_WritePin(LED_ORANGE_GPIO_Port,LED_ORANGE_Pin,RESET);
	HAL_Delay(8000);
	HAL_GPIO_WritePin(LED_GREEN_GPIO_Port,LED_GREEN_Pin,SET);
	HAL_GPIO_WritePin(LED_RED_GPIO_Port,LED_RED_Pin,SET);
	HAL_GPIO_WritePin(LED_ORANGE_GPIO_Port,LED_ORANGE_Pin,SET);


	SpiSetup();
	ReadNVRAll();

	if(!ChipAlreadyProgrammed)
	{
		EraseChip();
		WriteNVRAll();
		//UpdateTxPowerSettings();//Sorry, you need to do it in the hex or C file
		WriteZwaveCodeToFlash();
	}
	//WriteLockBits(8, 254);//sets read protection on the flash
	//ReadAllLockBits();
	//ReadNVRAll();
	WriteCRC();
	RunCRC();
	//ReadAllFlash();
	Reset();

	HAL_GPIO_WritePin(LED_ORANGE_GPIO_Port,LED_ORANGE_Pin,RESET);
	HAL_GPIO_WritePin(LED_GREEN_GPIO_Port,LED_GREEN_Pin,RESET);
	HAL_GPIO_WritePin(LED_RED_GPIO_Port,LED_RED_Pin,RESET);
	HAL_Delay(8000);
	HAL_GPIO_WritePin(LED_ORANGE_GPIO_Port,LED_ORANGE_Pin,SET);
	HAL_GPIO_WritePin(LED_GREEN_GPIO_Port,LED_GREEN_Pin,SET);
	HAL_GPIO_WritePin(LED_RED_GPIO_Port,LED_RED_Pin,SET);

	if(!CRC_Failed)
	{
		HAL_GPIO_WritePin(LED_RED_GPIO_Port,LED_RED_Pin,RESET);
	}

	HAL_Delay(3000);


	/*HAL_GPIO_WritePin(LED_GREEN_GPIO_Port,LED_GREEN_Pin,SET);
	HAL_GPIO_WritePin(LED_RED_GPIO_Port,LED_RED_Pin,SET);
	HAL_GPIO_WritePin(LED_ORANGE_GPIO_Port,LED_ORANGE_Pin,SET);*/
}

/*void HAL_UART_RxCpltCallback(UART_HandleTypeDef *huart)
{
	//HAL_Delay(30);
//	for(int i=0;i<65000;i++){}
	//uint8_t arr[]={6};
	HAL_GPIO_TogglePin(LED_GREEN_GPIO_Port,LED_GREEN_Pin);
	//HAL_UART_Transmit_IT(&ZW_Uart,&arr[0],1);

}*/
void HAL_SPI_TxRxCpltCallback(SPI_HandleTypeDef *hspi)
{
	    if(hspi->Instance == ZW_Spi.Instance )
		{

			/*0 ->x
			  1 ->172 ->0xAC
			  2 ->83  ->0x53
			  3 ->170 ->0XAA*/

	    	  if(SpiRxArr[2]==0x53 &&SpiRxArr[3]==0XAA )//SPI INITIALIZE
	    	  {
	    		   HAL_GPIO_WritePin(LED_RED_GPIO_Port,LED_RED_Pin,RESET);
	    			SyncDone =true;
	    	  }
	    	  if(SpiRxArr[1]==0X30 && SpiRxArr[2]==0X04 )//CHECK SIGNATURE
	       	  {
	    		  HAL_GPIO_WritePin(LED_RED_GPIO_Port,LED_RED_Pin,RESET);
	    		   SyncDone =true;
	    	  }


	    	  if(SpiRxArr[1]==0X7F && SpiRxArr[2]==0XFE )//FLASH STATE
	       	  {

	    		   SyncDone =true;
	    		   uint8_t bl = SpiRxArr[3] & 8;//flash busy

	    		   uint8_t bl2 = SpiRxArr[3] & 1;//CRC busy
	    		   uint8_t bl1 = SpiRxArr[3] & 2;//CRC done

	    		   uint8_t bl3 = SpiRxArr[3] & 4;//CRC failed or passed



						if(bl==0)
						{
							 FlashBusy=false;
						}
						else
						{
							faultInc--;

							 FlashBusy=true;
						}

						if(bl1==0)
						{
							CRCDone=false;
						}else
						{
							CRCDone=true;
						}


						if(bl2==0)
						{
							CRCbusy=false;
						}
						else
						{
							CRCbusy=true;
						}

						if(CRCDone && !CRCbusy && CRCcheckInProgress)
						{
							if(bl3==0)
							{
								CRC_Failed=false;
							}
							else
							{
								CRC_Failed=true;
							}
						}



	    	  }

	    	  if(SpiRxArr[1]==0xF1 &&SpiRxArr[2]==LockBitReg && Txdone)//READ LOCKBITS
		  {
		    	//HAL_GPIO_WritePin(LED_ORANGE_GPIO_Port,LED_ORANGE_Pin,RESET);
		    	 //HAL_UART_Transmit_IT(&ZW_Uart,&SpiRxArr[3],1);
		  }
		  if(SpiRxArr[1]==0xF2 &&SpiRxArr[2]==0x00 && Txdone)//READ NVR CONTENETS
		  {
		    	 //HAL_GPIO_WritePin(LED_ORANGE_GPIO_Port,LED_ORANGE_Pin,RESET);
		    	  nvrBytesZm5202[nvrcount] =SpiRxArr[3];
		    	  nvrcount++;
		    	 //HAL_UART_Transmit_IT(&ZW_Uart,&SpiRxArr[3],1);
		  }
	    	  if(contReadCmd  && Txdone)//Continue read checksum and read flash functions
		  {
		    		 // HAL_UART_Transmit_IT(&ZW_Uart,&SpiRxArr[1],3);
		    	  contReadCmd = false;
		  }



	    	 /* if(SpiRxArr[1]==0x04 &&SpiRxArr[2]==addh && Txdone )//WRITE SRAM
	    	  {
	    		  HAL_GPIO_WritePin(LED_ORANGE_GPIO_Port,LED_ORANGE_Pin,RESET);
	    	  }
	    	  if(SpiRxArr[1]==0x80  && Txdone )//CONTINUE WRITE SRAM
	    	  {
	    		  HAL_GPIO_WritePin(LED_ORANGE_GPIO_Port,LED_ORANGE_Pin,RESET);
	    	  }
	    	  if(SpiRxArr[1]==0x06 &&SpiRxArr[2]==addh && Txdone)//READ SRAM
	    	  {
	    		  HAL_GPIO_WritePin(LED_ORANGE_GPIO_Port,LED_ORANGE_Pin,RESET);
	    	  }
	    	  if(SpiRxArr[1]==0x20 &&SpiRxArr[2]==Sector && Txdone)//WRITE FLASH
	    	  {
	    	 	 HAL_GPIO_WritePin(LED_ORANGE_GPIO_Port,LED_ORANGE_Pin,RESET);
	    	 	 //HAL_UART_Transmit_IT(&ZW_Uart,&SpiRxArr[3],1);
	    	  }
	    	  if(SpiRxArr[1]==0x10 &&SpiRxArr[2]==Sector && Txdone)//READ FLASH
	    	  {
	    		  //harr[Sector] = SpiRxArr[3];
	    		  //HAL_UART_Transmit_IT(&ZW_Uart,&SpiRxArr[3],1);
	    		  //if(Sector>34)
	    		  //{
					  //HAL_GPIO_WritePin(LED_ORANGE_GPIO_Port,LED_ORANGE_Pin,RESET);
	    		  //}
	    	  }


	    	  if(SpiRxArr[1]==0xC3 &&SpiRxArr[2]==0x00 && Txdone)//CRC CHECK
	    	  {
	    	 	 HAL_GPIO_WritePin(LED_ORANGE_GPIO_Port,LED_ORANGE_Pin,RESET);
	    	 	 //HAL_UART_Transmit_IT(&ZW_Uart,&SpiRxArr[3],1);
	    	  }


	    	  *///testcode

	    	  /*if(SpiRxArr[0]==0X30 ||SpiRxArr[1]==0X30 ||SpiRxArr[2]==0X30||SpiRxArr[3]==0X30)
		    	  {
		    	  		HAL_GPIO_WritePin(LED_ORANGE_GPIO_Port,LED_ORANGE_Pin,RESET);
		    	  }
		    	  if(SpiRxArr[0]==0X7F ||SpiRxArr[1]==0X7F ||SpiRxArr[2]==0X7F||SpiRxArr[3]==0X7F)
			      {
			    	  	HAL_GPIO_WritePin(LED_ORANGE_GPIO_Port,LED_ORANGE_Pin,RESET);
			      }
				  if(b==4)
				  {
						HAL_GPIO_WritePin(LED_ORANGE_GPIO_Port,LED_ORANGE_Pin,RESET);
						b=0;
				  }*/


		}
}




